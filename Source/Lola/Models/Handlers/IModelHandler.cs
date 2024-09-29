namespace Lola.Models.Handlers;

public interface IModelHandler {
    ModelEntity[] List(Expression<Func<ModelEntity, bool>>? predicate = null);
    void IncludeProviders(IEnumerable<ModelEntity> models);
    ModelEntity? Find(Expression<Func<ModelEntity, bool>> predicate);
    void IncludeProvider(ModelEntity model);

    void Add(ModelEntity model);
    void Update(ModelEntity model);
    void Remove(uint id);

    void Select(uint id);
    ModelEntity? Selected { get; }
}
