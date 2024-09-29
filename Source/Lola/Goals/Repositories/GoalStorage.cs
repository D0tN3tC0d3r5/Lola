namespace Lola.Goals.Repositories;

public class GoalStorage(IConfiguration configuration)
    : JsonFilePerTypeStorage<GoalEntity>("goals", configuration),
      IGoalStorage;
