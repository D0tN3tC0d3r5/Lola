namespace Lola.Personas.Commands;

public class PersonasMainMenu(IHasChildren parent)
    : LolaCommand<PersonasMainMenu>(parent, "Personas", n => {
        n.Description = "Manage Agent's Personas";
        n.ErrorText = "displaying the persona's main menu";
        n.AddCommand<ListPersonas>();
        n.AddCommand<AddPersona>();
        n.AddCommand<ViewPersona>();
        n.AddCommand<UpdatePersona>();
        n.AddCommand<RemovePersona>();
        n.AddCommand<HelpCommand>();
        n.AddCommand<BackCommand>();
        n.AddCommand<ExitCommand>();
    }) {
    protected override async Task<Result> HandleCommandAsync(CancellationToken ct = default) {
        Logger.LogInformation("Executing Personas->Main command...");
        var choice = await Input.BuildSelectionPrompt("What would you like to do?")
                                .DisplayAs(MapTo)
                                .AddChoices(Commands.AsIndexed().OrderBy(i => i.Index).ToArray(c => c.Value.Name), ChoicePosition.AtEnd)
                                .ShowAsync(ct);

        var command = Commands.FirstOrDefault(i => i.Name == choice);
        return command is null
                   ? Result.Success()
                   : await command.Execute([], ct);

        string MapTo(string item) => Commands.FirstOrDefault(i => i.Name == item)?.Description ?? string.Empty;
    }
}
