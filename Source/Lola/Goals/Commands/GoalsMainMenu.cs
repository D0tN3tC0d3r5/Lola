namespace Lola.Goals.Commands;

public class GoalsMainMenu(IHasChildren parent)
    : LolaCommand<GoalsMainMenu>(parent, "Goals", n => {
        n.Description = "Manage Goals";
        n.ErrorText = "displaying the goal's main menu";
        n.AddCommand<ListGoals>();
        //n.AddCommand<TaskCreateCommand>();
        //n.AddCommand<TaskUpdateCommand>();
        //n.AddCommand<TaskRemoveCommand>();
        //AddCommand<TaskViewCommand>();
        n.AddCommand<HelpCommand>();
    }) {
    protected override async Task<Result> HandleCommandAsync(CancellationToken ct = default) {
        Logger.LogInformation("Executing Goals command...");
        var choice = await Input.BuildSelectionPrompt("What would you like to do?")
                                .DisplayAs(MapTo)
                                .AddChoices(Commands.ToArray(c => c.Name))
                                .ShowAsync(ct);

        var command = Commands.FirstOrDefault(i => i.Name == choice);
        return command is null
            ? Result.Success()
            : await command.Execute([], ct);

        string MapTo(string? item) => Commands.FirstOrDefault(i => i.Name == item)?.Description ?? string.Empty;
    }
}
