namespace Lola.Providers.Handlers;

public interface IProviderHandler {
    ProviderEntity[] List(Expression<Func<ProviderEntity, bool>>? predicate = null);
    bool Exists(Expression<Func<ProviderEntity, bool>> predicate);
    ProviderEntity? Find(Expression<Func<ProviderEntity, bool>> predicate);
    void Add(ProviderEntity provider);
    void Update(ProviderEntity provider);
    void Remove(uint id);
    void AddModel(uint id, ModelEntity model);
    void RemoveModel(uint id, uint modelId);
    void Enable(uint id);
    void Disable(uint id);
}
