namespace Lola.Jobs.Commands;

public class ViewJob(IHasChildren parent, IJobHandler handler)
    : LolaCommand<ViewJob>(parent, "Info", n => {
        n.Aliases = ["i"];
        n.Description = "View job details";
        n.ErrorText = "displaying the job information";
        n.Help = "Display detailed information about an agent's job.";
    }) {
    protected override async Task<Result> HandleCommandAsync(CancellationToken ct = default) {
        Logger.LogInformation("Executing Jobs->Info command...");
        var jobs = handler.List();
        if (jobs.Length == 0) {
            Output.WriteLine("[yellow]No jobs found.[/]");
            Logger.LogInformation("No jobs found. View job action cancelled.");
            return Result.Success();
        }
        var job = await this.SelectEntityAsync<JobEntity, uint>(jobs.OrderBy(p => p.Name), p => p.Name, ct);
        if (job is null) {
            Logger.LogInformation("No job selected.");
            return Result.Success();
        }

        ShowDetails(job);
        return Result.Success();
    }

    private void ShowDetails(JobEntity job) {
        Output.WriteLine($"{job.Name} [yellow]Information:[/]");
        Output.WriteLine();
        Output.WriteLine($"[blue]{nameof(JobEntity.Goals)}:[/]");
        foreach (var goal in job.Goals) Output.WriteLine($" - {goal}");
        Output.WriteLine();
    }
}
