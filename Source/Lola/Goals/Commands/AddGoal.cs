using Task = System.Threading.Tasks.Task;

namespace Lola.Goals.Commands;

public class AddGoal(IHasChildren parent, IGoalHandler goalHandler)
    : LolaCommand<AddGoal>(parent, "Generate", n => {
        n.Aliases = ["gen"];
        n.Description = "Generate a new goal";
        n.ErrorText = "generating the new goal";
        n.Help = "Generate a new agent goal using AI assistance.";
    }) {
    private const int _maxQuestions = 10;

    protected override async Task<Result> HandleCommandAsync(CancellationToken ct = default) {
        Logger.LogInformation("Executing Goals->Generate command...");
        var goal = new GoalEntity();
        await SetUpAsync(goal, ct);
        await AskAdditionalQuestions(goal, ct);
        await goalHandler.UpdateCreatedGoal(goal);

        Output.WriteLine($"[green]Agent goal '{goal.Name}' generated successfully.[/]");
        Logger.LogInformation("Goal '{GoalId}:{GoalName}' generated successfully.", goal.Id, goal.Name);

        ShowResult(goal);

        var saveGoal = await Input.ConfirmAsync("Are you ok with the generated Agent above?", ct);
        if (saveGoal) {
            goalHandler.Add(goal);
            Logger.LogInformation("Goal '{GoalId}:{GoalName}' added successfully.", goal.Id, goal.Name);
            return Result.Success();
        }

        Output.WriteLine("[yellow]Please review the provided answers and try again.[/]");
        return Result.Success();
    }

    private void ShowResult(GoalEntity goal) {
        Output.WriteLine();
        Output.WriteLine($"[teal]Name:[/] {goal.Name}");
        Output.WriteLine("[teal]Objectives:[/]");
        Output.WriteLine(string.Join("\n", goal.Objectives.Select(i => $" - {i}")));
        Output.WriteLine();
    }

    private async Task AskAdditionalQuestions(GoalEntity goal, CancellationToken ct) {
        for (var questionCount = 0; questionCount < _maxQuestions; questionCount++) {
            Output.WriteLine("[yellow]Let me see if I have more questions...[/]");
            Output.WriteLine("[grey](You can skip the questions by typing 'proceed' at any time.)[/]");

            var queries = await goalHandler.GenerateQuestion(goal);
            if (queries.Length == 0) {
                Output.WriteLine("[green]I've gathered sufficient information to generate the agent's goal.[/]");
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
                goal.Questions.Add(query);
            }

            if (!proceed) continue;
            Output.WriteLine("[green]Ok. Let's proceed with the Agent's Task generation.[/]");
            break;
        }
    }

    private async Task SetUpAsync(GoalEntity goal, CancellationToken ct) {
        goal.Name = await Input.BuildTextPrompt<string>("How would you like to call the Agent?")
                                  .AddValidation(name => GoalEntity.ValidateName(null, name, goalHandler))
                                  .ShowAsync(ct);
        var objective = await Input.BuildMultilinePrompt($"What is the Main Objective for the [white]{goal.Name}[/]?")
                              .AddValidation(GoalEntity.ValidateObjective)
                              .ShowAsync(ct);
        goal.Objectives.AddRange(objective.Replace("\r", "").Split("\n"));
        var addAnotherObjective = await Input.ConfirmAsync("Would you like to add another objective?", ct);
        while (addAnotherObjective) {
            objective = await Input.BuildMultilinePrompt("Additional objective: ")
                              .AddValidation(GoalEntity.ValidateObjective)
                              .ShowAsync(ct);
            goal.Objectives.AddRange(objective.Replace("\r", "").Split("\n"));
            addAnotherObjective = await Input.ConfirmAsync("Would you like to add another objective?", ct);
        }
    }
}
