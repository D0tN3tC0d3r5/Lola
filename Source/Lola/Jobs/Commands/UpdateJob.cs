using Task = System.Threading.Tasks.Task;

namespace Lola.Jobs.Commands;

public class UpdateJob(IHasChildren parent, IJobHandler handler)
    : LolaCommand<UpdateJob>(parent, "Update", n => {
        n.Aliases = ["edit"];
        n.Description = "Update a job";
        n.ErrorText = "updating the job";
        n.Help = "Update an existing agent's job.";
    }) {
    protected override async Task<Result> HandleCommandAsync(CancellationToken ct = default) {
        Logger.LogInformation("Executing Jobs->Update command...");
        var jobs = handler.List();
        if (jobs.Length == 0) {
            Output.WriteLine("[yellow]No jobs found.[/]");
            Logger.LogInformation("No jobs found. Update job action cancelled.");
            return Result.Success();
        }
        var job = await this.SelectEntityAsync<JobEntity, uint>(jobs.OrderBy(p => p.Name), p => p.Name, ct);
        if (job is null) {
            Logger.LogInformation("No job selected.");
            return Result.Success();
        }

        await SetUpAsync(job, ct);

        handler.Update(job);
        Output.WriteLine($"[green]Job '{job.Name}' updated successfully.[/]");
        Logger.LogInformation("Job '{JobId}:{JobName}' updated successfully.", job.Id, job.Name);
        return Result.Success();
    }

    private async Task SetUpAsync(JobEntity job, CancellationToken ct) {
        // Update Name
        job.Name = await Input.BuildTextPrompt<string>("- Name (ENTER to keep current):")
                                  .WithDefault(job.Name)
                                  .ShowOptionalFlag()
                                  .AddValidation(name => JobEntity.ValidateName(job.Id, name, handler))
                                  .ShowAsync(ct);

        // Update Goals
        Output.WriteLine($"This job has currently {job.Goals.Count} goals.");
        var goalCount = 0;
        while (goalCount < job.Goals.Count) {
            job.Goals[goalCount] = await Input.BuildMultilinePrompt($"- Goal {goalCount + 1}:")
                                                  .WithDefault(job.Goals[goalCount])
                                                  .AddValidation(JobEntity.ValidateGoal)
                                                  .ShowAsync(ct);
            goalCount++;
        }
        var addGoal = await Input.ConfirmAsync("Would you like to add another goal?", ct);
        while (addGoal) {
            job.Goals[goalCount] = await Input.BuildMultilinePrompt($"- Goal {goalCount + 1}:")
                                                  .AddValidation(JobEntity.ValidateGoal)
                                                  .ShowAsync(ct);
            addGoal = await Input.ConfirmAsync("Would you like to add another goal?", ct);
        }
    }
}
