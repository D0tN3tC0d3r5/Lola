using ValidationException = DotNetToolbox.Results.ValidationException;

namespace Lola.Providers.Handlers;

public class ProviderHandler(IProviderDataSource dataSource, Lazy<IModelHandler> modelHandler, ILogger<ProviderHandler> logger)
    : IProviderHandler {
    public ProviderEntity[] List(Expression<Func<ProviderEntity, bool>>? predicate = null) => dataSource.GetAll(predicate);

    public bool Exists(Expression<Func<ProviderEntity, bool>> predicate) => dataSource.Any(predicate);
    public ProviderEntity? Find(Expression<Func<ProviderEntity, bool>> predicate) => dataSource.Find(predicate);

    public void Add(ProviderEntity provider) {
        var context = Map.FromMap([new(nameof(ProviderHandler), this)]);
        var result = dataSource.Add(provider, context);
        if (!result.IsSuccess)
            throw new ValidationException(result.Errors);
        logger.LogInformation("Provider '{ProviderName} ({ProviderId})' added.", provider.Name, provider.Id);
    }

    public void Update(ProviderEntity provider) {
        EnsureExists(provider.Id);
        var context = Map.FromMap([new(nameof(ProviderHandler), this)]);
        var result = dataSource.Update(provider, context);
        if (!result.IsSuccess)
            throw new ValidationException(result.Errors);
        logger.LogInformation("Provider '{ProviderName} ({ProviderId})' updated.", provider.Name, provider.Id);
    }

    public void Remove(uint id) {
        var provider = EnsureExists(id);
        foreach (var model in modelHandler.Value.List(p => p.ProviderId == provider.Id))
            modelHandler.Value.Remove(model.Id);
        logger.LogInformation("Provider '{ProviderName} ({ProviderId})' models removed.", provider.Name, provider.Id);

        dataSource.Remove(id);
        logger.LogInformation("Provider '{ProviderName} ({ProviderId})' removed.", provider.Name, provider.Id);
    }

    public void AddModel(uint id, ModelEntity model) {
        EnsureExists(id);
        model.ProviderId = id;
        modelHandler.Value.Add(model);
    }

    public void RemoveModel(uint id, uint modelId) {
        EnsureExists(id);
        modelHandler.Value.Remove(modelId);
    }

    public void Enable(uint id) {
        var provider = EnsureExists(id);
        if (!provider.CanEnable) throw new ValidationException($"Provider '{id}' can't be enabled.");
        provider.IsEnabled = true;
        Update(provider);
    }

    public void Disable(uint id) {
        var provider = EnsureExists(id);
        provider.IsEnabled = false;
        Update(provider);
    }

    private ProviderEntity EnsureExists(uint id)
        => Find(p => p.Id == id) ?? throw new ValidationException($"Provider '{id}' not found.");
}
