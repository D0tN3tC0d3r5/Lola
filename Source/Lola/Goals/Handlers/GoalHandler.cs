using Task = System.Threading.Tasks.Task;

namespace Lola.Goals.Handlers;

public class GoalHandler(IServiceProvider services, ILogger<GoalHandler> logger)
    : IGoalHandler {
    private readonly IModelHandler _modelHandler = services.GetRequiredService<IModelHandler>();
    private readonly IUserProfileHandler _userHandler = services.GetRequiredService<IUserProfileHandler>();
    private readonly IGoalDataSource _dataSource = services.GetRequiredService<IGoalDataSource>();
    private readonly IPersonaHandler _personaHandler = services.GetRequiredService<IPersonaHandler>();
    private readonly IAgentAccessor _connectionAccessor = services.GetRequiredService<IAgentAccessor>();

    public GoalEntity[] List() => _dataSource.GetAll();

    public GoalEntity? GetById(uint id) => _dataSource.FindByKey(id);
    public GoalEntity? Find(Expression<Func<GoalEntity, bool>> predicate) => _dataSource.Find(predicate);

    public GoalEntity Create(Action<GoalEntity> setUp)
        => _dataSource.Create(setUp);

    public void Add(GoalEntity goal) {
        if (_dataSource.FindByKey(goal.Id) != null)
            throw new InvalidOperationException($"A goal with the id '{goal.Id}' already exists.");

        var context = Map.FromMap([new(nameof(GoalHandler), this)]);
        _dataSource.Add(goal, context);
        logger.LogInformation("Added new goal: {TaskId} => {TaskName}", goal.Name, goal.Id);
    }

    public void Update(GoalEntity goal) {
        if (_dataSource.FindByKey(goal.Id) == null)
            throw new InvalidOperationException($"Goal with id '{goal.Id}' not found.");

        var context = Map.FromMap([new(nameof(GoalHandler), this)]);
        _dataSource.Update(goal, context);
        logger.LogInformation("Updated goal: {TaskId} => {TaskName}", goal.Name, goal.Id);
    }

    public void Remove(uint id) {
        var task = _dataSource.FindByKey(id)
                     ?? throw new InvalidOperationException($"Goal with id '{id}' not found.");

        _dataSource.Remove(id);
        logger.LogInformation("Removed task: {TaskId} => {TaskName}", task.Name, task.Id);
    }

    public async Task<Query[]> GenerateQuestion(GoalEntity goal) {
        try {
            var appModel = _modelHandler.Selected ?? throw new InvalidOperationException("No default AI model selected.");
            var httpConnection = _connectionAccessor.GetFor(appModel.Provider!.Name);
            var userProfileEntity = _userHandler.CurrentUser ?? throw new InvalidOperationException("No user found.");
            var personaEntity = _personaHandler.GetById(1) ?? throw new InvalidOperationException("Required persona not found. Name: 'Task Creator'.");
            var goalEntity = GetById(1) ?? throw new InvalidOperationException("Required goal not found. Name: 'Ask Questions about the AI Task'.");
            var context = new JobContext {
                Model = appModel,
                Agent = httpConnection,
                UserProfile = userProfileEntity,
                Persona = personaEntity,
                Task = goalEntity,
                Input = goal,
            };
            var task = new Job(context);
            task.Converters.Add(typeof(List<Query>),
                                v => {
                                    var list = (List<Query>)v;
                                    if (list.Count == 0) return string.Empty;
                                    var sb = new StringBuilder();
                                    foreach (var item in list) {
                                        sb.AppendLine($"Q: {item.Question}");
                                        sb.AppendLine($"{item.Explanation}");
                                        sb.AppendLine($"A: {item.Answer}");
                                    }
                                    return sb.ToString();
                                });
            var result = await task.Execute(CancellationToken.None);
            if (result.HasException) throw new("Failed to generate next question: " + result.Exception.Message);
            var response = context.OutputAsMap.GetList<Map>("Questions");
            return response.ToArray(i => new Query {
                Question = i.GetRequiredValueAs<string>(nameof(Query.Question)),
                Explanation = i.GetRequiredValueAs<string>(nameof(Query.Explanation)),
            });
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error generating next question for goal {GoalName}", goal.Name);
            throw;
        }
    }

    public async Task UpdateCreatedGoal(GoalEntity goal) {
        try {
            var appModel = _modelHandler.Selected ?? throw new InvalidOperationException("No default AI model selected.");
            var httpConnection = _connectionAccessor.GetFor(appModel.Provider!.Name);
            var userProfileEntity = _userHandler.CurrentUser ?? throw new InvalidOperationException("No user found.");
            var personaEntity = _personaHandler.GetById(1) ?? throw new InvalidOperationException("Required goal not found. Name: 'Agent Creator'.");
            var goalEntity = GetById(2) ?? throw new InvalidOperationException("Required task not found. Name: 'Ask Questions about the AI Agent'.");
            var context = new JobContext {
                Model = appModel,
                Agent = httpConnection,
                UserProfile = userProfileEntity,
                Persona = personaEntity,
                Task = goalEntity,
                Input = goal,
            };
            var job = new Job(context);
            job.Converters.Add(typeof(List<Query>),
                                v => {
                                    var list = (List<Query>)v;
                                    if (list.Count == 0) return string.Empty;
                                    var sb = new StringBuilder();
                                    foreach (var item in list) {
                                        sb.AppendLine($"Q: {item.Question}");
                                        sb.AppendLine($"{item.Explanation}");
                                        sb.AppendLine($"A: {item.Answer}");
                                    }
                                    return sb.ToString();
                                });
            var result = await job.Execute(CancellationToken.None);
            if (result.HasException) throw new("Failed to generate next question: " + result.Exception.Message);
            goal.Objectives = context.OutputAsMap.GetRequiredList<string>(nameof(GoalEntity.Objectives));
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error generating next question for goal {GoalName}", goal.Name);
            throw;
        }
    }
}
