namespace Lola.Goals.Commands;

public class RemoveGoal(IHasChildren parent, IGoalHandler handler)
    : LolaCommand<RemoveGoal>(parent, "Remove", n => {
        n.Aliases = ["delete", "del"];
        n.Description = "Remove a goal";
        n.ErrorText = "removing the goal";
        n.Help = "Remove an existing agent's goal.";
    }) {
    protected override async Task<Result> HandleCommandAsync(CancellationToken ct = default) {
        Logger.LogInformation("Executing Goals->Remove command...");
        var goals = handler.List();
        if (goals.Length == 0) {
            Output.WriteLine("[yellow]No goals found.[/]");
            Logger.LogInformation("No goals found. Remove goal action cancelled.");
            return Result.Success();
        }
        var goal = await this.SelectEntityAsync<GoalEntity, uint>(goals.OrderBy(p => p.Name), p => p.Name, ct);
        if (goal is null) {
            Logger.LogInformation("No goal selected.");
            return Result.Success();
        }

        if (!await Input.ConfirmAsync($"Are you sure you want to remove the goal '{goal.Name}' ({goal.Id})?", ct)) {
            Logger.LogInformation("Goal removal cancelled by user.");
            return Result.Invalid("Action cancelled.");
        }

        handler.Remove(goal.Id);
        Output.WriteLine($"[green]Goal '{goal.Name}' removed successfully.[/]");
        Logger.LogInformation("Goal '{GoalId}:{GoalName}' removed successfully.", goal.Id, goal.Name);
        return Result.Success();
    }
}
