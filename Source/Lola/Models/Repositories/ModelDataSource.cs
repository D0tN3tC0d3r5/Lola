namespace Lola.Models.Repositories;

public class ModelDataSource(IModelStorage storage)
    : DataSource<IModelStorage, ModelEntity, uint>(storage),
      IModelDataSource;
