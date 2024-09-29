namespace Lola.Models.Repositories;

public class ModelStorage(IConfiguration configuration)
    : JsonFilePerTypeStorage<ModelEntity>("models", configuration),
      IModelStorage;
