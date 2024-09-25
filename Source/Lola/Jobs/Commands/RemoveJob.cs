namespace Lola.Jobs.Commands;

public class RemoveJob(IHasChildren parent, IJobHandler handler)
    : LolaCommand<RemoveJob>(parent, "Remove", n => {
        n.Aliases = ["delete", "del"];
        n.Description = "Remove a job";
        n.ErrorText = "removing the job";
        n.Help = "Remove an existing agent's job.";
    }) {
    protected override async Task<Result> HandleCommandAsync(CancellationToken ct = default) {
        Logger.LogInformation("Executing Jobs->Remove command...");
        var jobs = handler.List();
        if (jobs.Length == 0) {
            Output.WriteLine("[yellow]No jobs found.[/]");
            Logger.LogInformation("No jobs found. Remove job action cancelled.");
            return Result.Success();
        }
        var job = await this.SelectEntityAsync<JobEntity, uint>(jobs.OrderBy(p => p.Name), p => p.Name, ct);
        if (job is null) {
            Logger.LogInformation("No job selected.");
            return Result.Success();
        }

        if (!await Input.ConfirmAsync($"Are you sure you want to remove the job '{job.Name}' ({job.Id})?", ct)) {
            Logger.LogInformation("Job removal cancelled by user.");
            return Result.Invalid("Action cancelled.");
        }

        handler.Remove(job.Id);
        Output.WriteLine($"[green]Job '{job.Name}' removed successfully.[/]");
        Logger.LogInformation("Job '{JobId}:{JobName}' removed successfully.", job.Id, job.Name);
        return Result.Success();
    }
}
