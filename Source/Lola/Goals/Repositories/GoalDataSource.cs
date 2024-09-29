namespace Lola.Goals.Repositories;

public class GoalDataSource(IGoalStorage storage)
    : DataSource<IGoalStorage, GoalEntity, uint>(storage),
      IGoalDataSource;
