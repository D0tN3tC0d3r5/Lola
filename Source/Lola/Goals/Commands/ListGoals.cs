namespace Lola.Goals.Commands;

public class ListGoals(IHasChildren parent, IGoalHandler goalHandler)
    : LolaCommand<ListGoals>(parent, "List", n => {
        n.Aliases = ["ls"];
        n.ErrorText = "listing the goals";
        n.Description = "List the existing goals.";
    }) {
    protected override Result HandleCommand() {
        Logger.LogInformation("Executing Goals->List command...");
        var goals = goalHandler.List();
        if (goals.Length == 0) {
            Output.WriteLine("[yellow]No goals found.[/]");
            return Result.Success();
        }

        var sortedGoals = goals.OrderBy(m => m.Name);
        ShowList(sortedGoals);

        return Result.Success();
    }

    private void ShowList(IOrderedEnumerable<GoalEntity> sortedGoals) {
        var table = new Table();
        table.Expand();
        table.AddColumn(new("[yellow]Name[/]"));
        table.AddColumn(new("[yellow]Main Objective[/]"));
        foreach (var goal in sortedGoals)
            table.AddRow(goal.Name, goal.Objectives.FirstOrDefault() ?? "[red][Undefined][/]");
        Output.Write(table);
    }
}
