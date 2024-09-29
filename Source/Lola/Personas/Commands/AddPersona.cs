using Task = System.Threading.Tasks.Task;

namespace Lola.Personas.Commands;

public class AddPersona(IHasChildren parent, IPersonaHandler personaHandler)
    : LolaCommand<AddPersona>(parent, "Generate", n => {
        n.Aliases = ["gen"];
        n.Description = "Generate a new persona";
        n.ErrorText = "generating the new persona";
        n.Help = "Generate a new agent persona using AI assistance.";
    }) {
    private const int _maxQuestions = 10;

    protected override async Task<Result> HandleCommandAsync(CancellationToken ct = default) {
        Logger.LogInformation("Executing Personas->Generate command...");
        var persona = new PersonaEntity();
        await SetUpAsync(persona, ct);
        await AskAdditionalQuestions(persona, ct);
        await personaHandler.UpdateCreatedPersona(persona);

        Output.WriteLine($"[green]Agent persona '{persona.Name}' generated successfully.[/]");
        Logger.LogInformation("Persona '{PersonaId}:{PersonaName}' generated successfully.", persona.Id, persona.Name);

        ShowResult(persona);

        var savePersona = await Input.ConfirmAsync("Are you ok with the generated Agent above?", ct);
        if (savePersona) {
            personaHandler.Add(persona);
            Logger.LogInformation("Persona '{PersonaId}:{PersonaName}' added successfully.", persona.Id, persona.Name);
            return Result.Success();
        }

        Output.WriteLine("[yellow]Please review the provided answers and try again.[/]");
        return Result.Success();
    }

    private void ShowResult(PersonaEntity persona) {
        Output.WriteLine();
        Output.WriteLine($"[teal]Name:[/] {persona.Name}");
        Output.WriteLine($"[teal]Role:[/] {persona.Role}");
        Output.WriteLine("[teal]Objectives:[/]");
        Output.WriteLine(string.Join("\n", persona.Objectives.Select(i => $" - {i}")));
        Output.WriteLine("[teal]Expertise:[/] [green](auto-generated)[/]");
        Output.WriteLine(persona.Expertise);
        Output.WriteLine("[teal]Characteristics:[/] [green](auto-generated)[/]");
        Output.WriteLine(string.Join("\n", persona.Characteristics.Select(i => $" - {i}")));
        Output.WriteLine("[teal]Requirements:[/] [green](auto-generated)[/]");
        Output.WriteLine(string.Join("\n", persona.Requirements.Select(i => $" - {i}")));
        Output.WriteLine("[teal]Restrictions:[/] [green](auto-generated)[/]");
        Output.WriteLine(string.Join("\n", persona.Restrictions.Select(i => $" - {i}")));
        Output.WriteLine("[teal]Traits:[/] [green](auto-generated)[/]");
        Output.WriteLine(string.Join("\n", persona.Traits.Select(i => $" - {i}")));
        Output.WriteLine();
    }

    private async Task AskAdditionalQuestions(PersonaEntity persona, CancellationToken ct) {
        for (var questionCount = 0; questionCount < _maxQuestions; questionCount++) {
            Output.WriteLine("[yellow]Let me see if I have more questions...[/]");
            Output.WriteLine("[grey](You can skip the questions by typing 'proceed' at any time.)[/]");

            var queries = await personaHandler.GenerateQuestion(persona);
            if (queries.Length == 0) {
                Output.WriteLine("[green]I've gathered sufficient information to generate the agent's persona.[/]");
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
                persona.Questions.Add(query);
            }

            if (!proceed) continue;
            Output.WriteLine("[green]Ok. Let's proceed with the Agent's Persona generation.[/]");
            break;
        }
    }

    private async Task SetUpAsync(PersonaEntity persona, CancellationToken ct) {
        persona.Name = await Input.BuildTextPrompt<string>("How would you like to call the Agent?")
                                  .AddValidation(name => PersonaEntity.ValidateName(null, name, personaHandler))
                                  .ShowAsync(ct);
        persona.Role = await Input.BuildTextPrompt<string>($"What is the [white]{persona.Name}[/] primary role?")
                                  .AddValidation(PersonaEntity.ValidateRole)
                                  .ShowAsync(ct);

        var objective = await Input.BuildMultilinePrompt($"What is the Main Objective for the [white]{persona.Name}[/]?")
                              .AddValidation(PersonaEntity.ValidateObjective)
                              .ShowAsync(ct);
        persona.Objectives.AddRange(objective.Replace("\r", "").Split("\n"));
        var addAnotherObjective = await Input.ConfirmAsync("Would you like to add another objective?", ct);
        while (addAnotherObjective) {
            objective = await Input.BuildMultilinePrompt("Additional objective: ")
                              .AddValidation(PersonaEntity.ValidateObjective)
                              .ShowAsync(ct);
            persona.Objectives.AddRange(objective.Replace("\r", "").Split("\n"));
            addAnotherObjective = await Input.ConfirmAsync("Would you like to add another objective?", ct);
        }
    }
}
