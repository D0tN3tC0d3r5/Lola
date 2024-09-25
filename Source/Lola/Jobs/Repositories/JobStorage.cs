namespace Lola.Jobs.Repositories;

public class JobStorage(IConfiguration configuration)
    : JsonFilePerTypeStorage<JobEntity, uint>("tasks", configuration),
      IJobStorage {
    protected override uint FirstKey { get; } = 1;

    protected override bool TryGenerateNextKey(out uint next) {
        next = LastUsedKey == default ? FirstKey : ++LastUsedKey;
        return true;
    }
}
