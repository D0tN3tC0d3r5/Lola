namespace Lola.Providers.Repositories;

public sealed class ProviderStorage(IConfiguration configuration)
    : JsonFilePerTypeStorage<ProviderEntity>("providers", configuration),
      IProviderStorage;
