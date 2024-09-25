using Task = DotNetToolbox.AI.Jobs.Task;

namespace Lola.Jobs.Repositories;

public class JobEntity
    : Entity<JobEntity, uint> {
    public string Name { get; set; } = string.Empty;
    public List<string> Goals { get; set; } = [];

    public List<Query> Questions { get; init; } = [];

    public List<string> Scope { get; init; } = [];
    public List<string> Requirements { get; init; } = [];
    public List<string> Assumptions { get; init; } = [];
    public List<string> Constraints { get; init; } = [];
    public List<string> Examples { get; init; } = [];
    public List<string> Guidelines { get; init; } = [];
    public List<string> Validations { get; init; } = [];

    public string InputTemplate { get; init; } = string.Empty;
    public TaskResponseType ResponseType { get; init; }
    public string ResponseSchema { get; init; } = string.Empty;

    public override Result Validate(IMap? context = null) {
        var result = base.Validate(context);
        var action = IsNotNull(context).GetRequiredValueAs<EntityAction>(nameof(EntityAction));
        result += ValidateName(action == EntityAction.Insert ? null : Id, Name, context.GetRequiredValueAs<IJobHandler>(nameof(JobHandler)));
        result += ValidateGoals(Goals);
        return result;
    }

    public static Result ValidateName(uint? id, string? name, IJobHandler handler) {
        var result = Result.Success();
        if (string.IsNullOrWhiteSpace(name))
            result += new ValidationError("The name is required.", nameof(Name));
        else if (handler.Find(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && (id == null || p.Id != id)) is not null)
            result += new ValidationError("A job with this name is already registered.", nameof(Name));
        return result;
    }

    public static Result ValidateGoal(string? goal) {
        var result = Result.Success();
        if (string.IsNullOrWhiteSpace(goal))
            result += new ValidationError("The goal cannot be null or empty.", nameof(Goals));
        return result;
    }

    public static Result ValidateGoals(List<string> goals) {
        var result = Result.Success();
        if (goals.Count == 0)
            result += new ValidationError("At least one goal is required.", nameof(Goals));
        return goals.Aggregate(result, (current, goal) => current + ValidateGoal(goal));
    }

    public static implicit operator Task(JobEntity entity)
        => new(entity.Id) {
            Name = entity.Name,
            Goals = entity.Goals,
            Scope = entity.Scope,
            Requirements = entity.Requirements,
            Assumptions = entity.Assumptions,
            Constraints = entity.Constraints,
            Examples = entity.Examples,
            Guidelines = entity.Guidelines,
            Validations = entity.Validations,
            InputTemplate = entity.InputTemplate,
            ResponseType = entity.ResponseType,
            ResponseSchema = entity.ResponseSchema,
        };
}
