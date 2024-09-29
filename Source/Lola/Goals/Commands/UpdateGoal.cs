using Task = System.Threading.Tasks.Task;

namespace Lola.Goals.Commands;

public class UpdateGoal(IHasChildren parent, IGoalHandler handler)
    : LolaCommand<UpdateGoal>(parent, "Update", n => {
        n.Aliases = ["edit"];
        n.Description = "Update a goal";
        n.ErrorText = "updating the goal";
        n.Help = "Update an existing agent's goal.";
    }) {
    protected override async Task<Result> HandleCommandAsync(CancellationToken ct = default) {
        Logger.LogInformation("Executing Goals->Update command...");
        var goals = handler.List();
        if (goals.Length == 0) {
            Output.WriteLine("[yellow]No goals found.[/]");
            Logger.LogInformation("No goals found. Update goal action cancelled.");
            return Result.Success();
        }
        var goal = await this.SelectEntityAsync<GoalEntity, uint>(goals.OrderBy(p => p.Name), p => p.Name, ct);
        if (goal is null) {
            Logger.LogInformation("No goal selected.");
            return Result.Success();
        }

        await SetUpAsync(goal, ct);

        handler.Update(goal);
        Output.WriteLine($"[green]Goal '{goal.Name}' updated successfully.[/]");
        Logger.LogInformation("Goal '{GoalId}:{GoalName}' updated successfully.", goal.Id, goal.Name);
        return Result.Success();
    }

    private async Task SetUpAsync(GoalEntity goal, CancellationToken ct) {
        // Update Name
        goal.Name = await Input.BuildTextPrompt<string>("- Name (ENTER to keep current):")
                                  .WithDefault(goal.Name)
                                  .ShowOptionalFlag()
                                  .AddValidation(name => GoalEntity.ValidateName(goal.Id, name, handler))
                                  .ShowAsync(ct);

        // Update Objectives
        Output.WriteLine($"This goal has currently {goal.Objectives.Count} objectives.");
        var objectiveCount = 0;
        while (objectiveCount < goal.Objectives.Count) {
            goal.Objectives[objectiveCount] = await Input.BuildMultilinePrompt($"- Objective {objectiveCount + 1}:")
                                                  .WithDefault(goal.Objectives[objectiveCount])
                                                  .AddValidation(GoalEntity.ValidateObjective)
                                                  .ShowAsync(ct);
            objectiveCount++;
        }
        var addObjective = await Input.ConfirmAsync("Would you like to add another objective?", ct);
        while (addObjective) {
            goal.Objectives[objectiveCount] = await Input.BuildMultilinePrompt($"- Objective {objectiveCount + 1}:")
                                                  .AddValidation(GoalEntity.ValidateObjective)
                                                  .ShowAsync(ct);
            addObjective = await Input.ConfirmAsync("Would you like to add another objective?", ct);
        }
    }
}
