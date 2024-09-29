using Task = DotNetToolbox.AI.Jobs.Task;

namespace Lola.Goals;

public class GoalEntity
    : Entity<GoalEntity, uint> {
    public string Name { get; set; } = string.Empty;
    public List<string> Objectives { get; set; } = [];

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
        result += ValidateName(action == EntityAction.Insert ? null : Id, Name, context.GetRequiredValueAs<IGoalHandler>(nameof(GoalHandler)));
        result += ValidateObjectives(Objectives);
        return result;
    }

    public static Result ValidateName(uint? id, string? name, IGoalHandler handler) {
        var result = Result.Success();
        if (string.IsNullOrWhiteSpace(name))
            result += new ValidationError("The name is required.", nameof(Name));
        else if (handler.Find(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && (id == null || p.Id != id)) is not null)
            result += new ValidationError("A goal with this name is already registered.", nameof(Name));
        return result;
    }

    public static Result ValidateObjective(string? objective) {
        var result = Result.Success();
        if (string.IsNullOrWhiteSpace(objective))
            result += new ValidationError("The objective cannot be null or empty.", nameof(Objectives));
        return result;
    }

    public static Result ValidateObjectives(List<string> objectives) {
        var result = Result.Success();
        if (objectives.Count == 0)
            result += new ValidationError("At least one objective is required.", nameof(Objectives));
        return objectives.Aggregate(result, (current, objective) => current + ValidateObjective(objective));
    }

    public static implicit operator Task(GoalEntity entity)
        => new(entity.Id) {
            Name = entity.Name,
            Goals = entity.Objectives,
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
