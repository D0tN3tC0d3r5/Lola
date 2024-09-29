using ValidationException = DotNetToolbox.Results.ValidationException;

namespace Lola.Models.Handlers;

public class ModelHandler(IApplication application, IModelDataSource dataSource, Lazy<IProviderHandler> providerHandler, ILogger<ModelHandler> logger)
    : IModelHandler {
    private const string _applicationModelKey = "ApplicationModel";

    private ModelEntity? _selected;

    public ModelEntity? Selected {
        get => GetSelected();
        private set => SetSelected(IsNotNull(value));
    }

    private ModelEntity? GetSelected() {
        var cachedValue = application.Context.GetValueAs<ModelEntity>(_applicationModelKey);
        _selected = cachedValue ?? dataSource.Find(m => m.Selected);
        if (_selected is null) return null;
        if (cachedValue is null) application.Context[_applicationModelKey] = _selected;
        return _selected; // Should only return null if the storage is empty or there is no selected model in the storage.
    }

    private void SetSelected(ModelEntity value) {
        if (value.Key == _selected?.Key) return;
        _selected = value;

        // Ensure record uniqueness in storage
        var oldSelectedModel = dataSource.Find(m => m.Selected);
        if (oldSelectedModel is not null && oldSelectedModel.Key != _selected.Key) {
            oldSelectedModel.Selected = false;
            dataSource.Update(oldSelectedModel);
        }
        _selected.Selected = true;
        dataSource.Update(_selected);

        // Update cached value
        application.Context[_applicationModelKey] = _selected;
    }

    public ModelEntity[] List(Expression<Func<ModelEntity, bool>>? predicate = null)
        => [.. dataSource.GetAll(predicate).OrderBy(m => m.Name)];

    public void IncludeProviders(IEnumerable<ModelEntity> models) {
        var providers = new Dictionary<uint, ProviderEntity?>();
        foreach (var model in models) {
            if (!providers.ContainsKey(model.ProviderId)) providers.Add(model.ProviderId, providerHandler.Value.Find(p => p.Id == model.ProviderId));
            model.Provider = providers[model.ProviderId];
        }
    }

    public ModelEntity? Find(Expression<Func<ModelEntity, bool>> predicate)
        => dataSource.Find(predicate);

    public void IncludeProvider(ModelEntity model) => model.Provider = providerHandler.Value.Find(p => p.Id == model.ProviderId);

    public void Add(ModelEntity model) {
        if (_selected is null) model.Selected = true;
        var context = Map.FromMap([new(nameof(ModelHandler), this), new(nameof(ProviderHandler), providerHandler.Value)]);
        var result = dataSource.Add(model, context);
        if (!result.IsSuccess)
            throw new ValidationException(result.Errors);
        _selected = model;
        logger.LogInformation("Model '{ModelName} ({ModelId})' added.", model.Name, model.Id);
    }

    public void Update(ModelEntity model) {
        EnsureExists(model.Id);
        var context = Map.FromMap([new(nameof(ModelHandler), this), new(nameof(ProviderHandler), providerHandler.Value)]);
        var result = dataSource.Update(model, context);
        if (!result.IsSuccess)
            throw new ValidationException(result.Errors);
        logger.LogInformation("Model '{ModelName} ({ModelId})' updated.", model.Name, model.Id);
    }

    public void Remove(uint id) {
        var model = EnsureExists(id);
        dataSource.Remove(id);
        logger.LogInformation("Model '{ModelName} ({ModelId})' removed.", model.Name, model.Id);
    }

    public ModelEntity[] ListByProvider(uint providerKey) => dataSource.GetAll(m => m.ProviderId == providerKey);

    public void Select(uint id) {
        var model = EnsureExists(id);
        Selected = model;
        logger.LogInformation("Model '{ModelName} ({ModelId})' selected : ", model.Name, model.Id);
    }

    private ModelEntity EnsureExists(uint id)
        => Find(p => p.Id == id) ?? throw new ValidationException($"Model '{id}' not found.");
}
