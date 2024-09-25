namespace Lola.Jobs.Repositories;

public class JobDataSource(IJobStorage storage)
    : DataSource<IJobStorage, JobEntity, uint>(storage),
      IJobDataSource;
