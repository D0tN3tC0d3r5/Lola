using Task = System.Threading.Tasks.Task;

namespace Lola.Jobs.Commands;

public class AddJob(IHasChildren parent, IJobHandler jobHandler)
    : LolaCommand<AddJob>(parent, "Generate", n => {
        n.Aliases = ["gen"];
        n.Description = "Generate a new job";
        n.ErrorText = "generating the new job";
        n.Help = "Generate a new agent job using AI assistance.";
    }) {
    private const int _maxQuestions = 10;

    protected override async Task<Result> HandleCommandAsync(CancellationToken ct = default) {
        Logger.LogInformation("Executing Jobs->Generate command...");
        var job = new JobEntity();
        await SetUpAsync(job, ct);
        await AskAdditionalQuestions(job, ct);
        await jobHandler.UpdateCreatedJob(job);

        Output.WriteLine($"[green]Agent job '{job.Name}' generated successfully.[/]");
        Logger.LogInformation("Job '{JobId}:{JobName}' generated successfully.", job.Id, job.Name);

        ShowResult(job);

        var saveJob = await Input.ConfirmAsync("Are you ok with the generated Agent above?", ct);
        if (saveJob) {
            jobHandler.Add(job);
            Logger.LogInformation("Job '{JobId}:{JobName}' added successfully.", job.Id, job.Name);
            return Result.Success();
        }

        Output.WriteLine("[yellow]Please review the provided answers and try again.[/]");
        return Result.Success();
    }

    private void ShowResult(JobEntity job) {
        Output.WriteLine();
        Output.WriteLine($"[teal]Name:[/] {job.Name}");
        Output.WriteLine("[teal]Goals:[/]");
        Output.WriteLine(string.Join("\n", job.Goals.Select(i => $" - {i}")));
        Output.WriteLine();
    }

    private async Task AskAdditionalQuestions(JobEntity job, CancellationToken ct) {
        for (var questionCount = 0; questionCount < _maxQuestions; questionCount++) {
            Output.WriteLine("[yellow]Let me see if I have more questions...[/]");
            Output.WriteLine("[grey](You can skip the questions by typing 'proceed' at any time.)[/]");

            var queries = await jobHandler.GenerateQuestion(job);
            if (queries.Length == 0) {
                Output.WriteLine("[green]I've gathered sufficient information to generate the agent's job.[/]");
                break;
            }
            var proceed = false;
            foreach (var query in queries) {
                query.Answer = await Input.BuildMultilinePrompt($"Question {questionCount + 1}: {query.Question}")
                                          .ShowAsync(ct);
                if (query.Answer.Equals("proceed", StringComparison.OrdinalIgnoreCase)) {
                    proceed = true;
                    break;
                }
                job.Questions.Add(query);
            }

            if (!proceed) continue;
            Output.WriteLine("[green]Ok. Let's proceed with the Agent's Task generation.[/]");
            break;
        }
    }

    private async Task SetUpAsync(JobEntity job, CancellationToken ct) {
        job.Name = await Input.BuildTextPrompt<string>("How would you like to call the Agent?")
                                  .AddValidation(name => JobEntity.ValidateName(null, name, jobHandler))
                                  .ShowAsync(ct);
        var goal = await Input.BuildMultilinePrompt($"What is the Main Goal for the [white]{job.Name}[/]?")
                              .AddValidation(JobEntity.ValidateGoal)
                              .ShowAsync(ct);
        job.Goals.AddRange(goal.Replace("\r", "").Split("\n"));
        var addAnotherGoal = await Input.ConfirmAsync("Would you like to add another goal?", ct);
        while (addAnotherGoal) {
            goal = await Input.BuildMultilinePrompt("Additional goal: ")
                              .AddValidation(JobEntity.ValidateGoal)
                              .ShowAsync(ct);
            job.Goals.AddRange(goal.Replace("\r", "").Split("\n"));
            addAnotherGoal = await Input.ConfirmAsync("Would you like to add another goal?", ct);
        }
    }
}
