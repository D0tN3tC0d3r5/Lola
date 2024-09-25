using Task = System.Threading.Tasks.Task;

namespace Lola.Jobs.Handlers;

public interface IJobHandler {
    JobEntity[] List();
    JobEntity? GetById(uint id);
    JobEntity? Find(Expression<Func<JobEntity, bool>> predicate);
    JobEntity Create(Action<JobEntity> setUp);
    void Add(JobEntity job);
    void Update(JobEntity job);
    void Remove(uint id);

    Task<Query[]> GenerateQuestion(JobEntity job);
    Task UpdateCreatedJob(JobEntity job);
}
