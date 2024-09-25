using Task = System.Threading.Tasks.Task;

namespace Lola.Jobs.Handlers;

public class JobHandler(IServiceProvider services, ILogger<JobHandler> logger)
    : IJobHandler {
    private readonly IModelHandler _modelHandler = services.GetRequiredService<IModelHandler>();
    private readonly IUserProfileHandler _userHandler = services.GetRequiredService<IUserProfileHandler>();
    private readonly IJobDataSource _dataSource = services.GetRequiredService<IJobDataSource>();
    private readonly IPersonaHandler _personaHandler = services.GetRequiredService<IPersonaHandler>();
    private readonly IAgentAccessor _connectionAccessor = services.GetRequiredService<IAgentAccessor>();

    public JobEntity[] List() => _dataSource.GetAll();

    public JobEntity? GetById(uint id) => _dataSource.FindByKey(id);
    public JobEntity? Find(Expression<Func<JobEntity, bool>> predicate) => _dataSource.Find(predicate);

    public JobEntity Create(Action<JobEntity> setUp)
        => _dataSource.Create(setUp);

    public void Add(JobEntity job) {
        if (_dataSource.FindByKey(job.Id) != null)
            throw new InvalidOperationException($"A job with the id '{job.Id}' already exists.");

        var context = Map.FromMap([new(nameof(JobHandler), this)]);
        _dataSource.Add(job, context);
        logger.LogInformation("Added new job: {TaskId} => {TaskName}", job.Name, job.Id);
    }

    public void Update(JobEntity job) {
        if (_dataSource.FindByKey(job.Id) == null)
            throw new InvalidOperationException($"Job with id '{job.Id}' not found.");

        var context = Map.FromMap([new(nameof(JobHandler), this)]);
        _dataSource.Update(job, context);
        logger.LogInformation("Updated job: {TaskId} => {TaskName}", job.Name, job.Id);
    }

    public void Remove(uint id) {
        var task = _dataSource.FindByKey(id)
                     ?? throw new InvalidOperationException($"Job with id '{id}' not found.");

        _dataSource.Remove(id);
        logger.LogInformation("Removed task: {TaskId} => {TaskName}", task.Name, task.Id);
    }

    public async Task<Query[]> GenerateQuestion(JobEntity job) {
        try {
            var appModel = _modelHandler.Selected ?? throw new InvalidOperationException("No default AI model selected.");
            var httpConnection = _connectionAccessor.GetFor(appModel.Provider!.Name);
            var userProfileEntity = _userHandler.CurrentUser ?? throw new InvalidOperationException("No user found.");
            var personaEntity = _personaHandler.GetById(1) ?? throw new InvalidOperationException("Required persona not found. Name: 'Task Creator'.");
            var jobEntity = GetById(1) ?? throw new InvalidOperationException("Required job not found. Name: 'Ask Questions about the AI Task'.");
            var context = new JobContext {
                Model = appModel,
                Agent = httpConnection,
                UserProfile = userProfileEntity,
                Persona = personaEntity,
                Task = jobEntity,
                Input = job,
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
            logger.LogError(ex, "Error generating next question for job {JobName}", job.Name);
            throw;
        }
    }

    public async Task UpdateCreatedJob(JobEntity job) {
        try {
            var appModel = _modelHandler.Selected ?? throw new InvalidOperationException("No default AI model selected.");
            var httpConnection = _connectionAccessor.GetFor(appModel.Provider!.Name);
            var userProfileEntity = _userHandler.CurrentUser ?? throw new InvalidOperationException("No user found.");
            var personaEntity = _personaHandler.GetById(1) ?? throw new InvalidOperationException("Required job not found. Name: 'Agent Creator'.");
            var jobEntity = GetById(2) ?? throw new InvalidOperationException("Required task not found. Name: 'Ask Questions about the AI Agent'.");
            var context = new JobContext {
                Model = appModel,
                Agent = httpConnection,
                UserProfile = userProfileEntity,
                Persona = personaEntity,
                Task = jobEntity,
                Input = job,
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
            job.Goals = context.OutputAsMap.GetRequiredList<string>(nameof(JobEntity.Goals));
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error generating next question for job {JobName}", job.Name);
            throw;
        }
    }
}
