using Task = System.Threading.Tasks.Task;

namespace Lola.Goals.Handlers;

public interface IGoalHandler {
    GoalEntity[] List();
    GoalEntity? GetById(uint id);
    GoalEntity? Find(Expression<Func<GoalEntity, bool>> predicate);
    GoalEntity Create(Action<GoalEntity> setUp);
    void Add(GoalEntity goal);
    void Update(GoalEntity goal);
    void Remove(uint id);

    Task<Query[]> GenerateQuestion(GoalEntity goal);
    Task UpdateCreatedGoal(GoalEntity goal);
}
