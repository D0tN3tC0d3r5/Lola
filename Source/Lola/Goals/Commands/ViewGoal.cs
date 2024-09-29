namespace Lola.Goals.Commands;

public class ViewGoal(IHasChildren parent, IGoalHandler handler)
    : LolaCommand<ViewGoal>(parent, "Info", n => {
        n.Aliases = ["i"];
        n.Description = "View goal details";
        n.ErrorText = "displaying the goal information";
        n.Help = "Display detailed information about an agent's goal.";
    }) {
    protected override async Task<Result> HandleCommandAsync(CancellationToken ct = default) {
        Logger.LogInformation("Executing Goals->Info command...");
        var goals = handler.List();
        if (goals.Length == 0) {
            Output.WriteLine("[yellow]No goals found.[/]");
            Logger.LogInformation("No goals found. View goal action cancelled.");
            return Result.Success();
        }
        var goal = await this.SelectEntityAsync<GoalEntity, uint>(goals.OrderBy(p => p.Name), p => p.Name, ct);
        if (goal is null) {
            Logger.LogInformation("No goal selected.");
            return Result.Success();
        }

        ShowDetails(goal);
        return Result.Success();
    }

    private void ShowDetails(GoalEntity goal) {
        Output.WriteLine($"{goal.Name} [yellow]Information:[/]");
        Output.WriteLine();
        Output.WriteLine($"[blue]{nameof(GoalEntity.Objectives)}:[/]");
        foreach (var objective in goal.Objectives) Output.WriteLine($" - {objective}");
        Output.WriteLine();
    }
}
