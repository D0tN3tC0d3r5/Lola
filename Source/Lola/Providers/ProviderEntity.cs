namespace Lola.Providers;

public class ProviderEntity
    : Entity<ProviderEntity, uint> {
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string? ApiKey { get; set; }

    public override Result Validate(IMap? context = null) {
        var result = base.Validate(context);
        var action = IsNotNull(context).GetRequiredValueAs<EntityAction>(nameof(EntityAction));
        result += ValidateName(action == EntityAction.Insert ? null : Id, Name, context.GetRequiredValueAs<IProviderHandler>(nameof(ProviderHandler)));
        return result;
    }

    public static Result ValidateName(uint? id, string? name, IProviderHandler handler) {
        var result = Result.Success();
        if (string.IsNullOrWhiteSpace(name))
            result += new ValidationError("A valid name is required.", nameof(Name));
        else if (handler.Find(p => (id == null || p.Id != id) && p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) is not null)
            result += new ValidationError("The provider name must be unique. A provider with this name is already registered.", nameof(Name));
        return result;
    }

    public static Result ValidateApiKey(uint? id, string? apiKey, IProviderHandler handler) {
        var result = Result.Success();
        if (string.IsNullOrWhiteSpace(apiKey))
            result += new ValidationError("A valid API Key is required.", nameof(Name));
        else if (handler.Find(p => (id == null || p.Id != id) && p.ApiKey != null && p.ApiKey.Equals(apiKey, StringComparison.OrdinalIgnoreCase)) is not null)
            result += new ValidationError("The provider name must be unique. A provider with this name is already registered.", nameof(Name));
        return result;
    }

    public bool CanEnable => string.IsNullOrWhiteSpace(ApiKey);
}
