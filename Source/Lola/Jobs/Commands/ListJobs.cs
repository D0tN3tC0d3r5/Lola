namespace Lola.Jobs.Commands;

public class ListJobs(IHasChildren parent, IJobHandler jobHandler)
    : LolaCommand<ListJobs>(parent, "List", n => {
        n.Aliases = ["ls"];
        n.ErrorText = "listing the jobs";
        n.Description = "List the existing jobs.";
    }) {
    protected override Result HandleCommand() {
        Logger.LogInformation("Executing Jobs->List command...");
        var jobs = jobHandler.List();
        if (jobs.Length == 0) {
            Output.WriteLine("[yellow]No jobs found.[/]");
            return Result.Success();
        }

        var sortedJobs = jobs.OrderBy(m => m.Name);
        ShowList(sortedJobs);

        return Result.Success();
    }

    private void ShowList(IOrderedEnumerable<JobEntity> sortedJobs) {
        var table = new Table();
        table.Expand();
        table.AddColumn(new("[yellow]Name[/]"));
        table.AddColumn(new("[yellow]Main Goal[/]"));
        foreach (var job in sortedJobs)
            table.AddRow(job.Name, job.Goals.FirstOrDefault() ?? "[red][Undefined][/]");
        Output.Write(table);
    }
}
