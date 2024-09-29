namespace Lola.Models.Commands;

public class ModelsMainMenu(IHasChildren parent)
    : LolaCommand<ModelsMainMenu>(parent, "Models", n => {
        n.Description = "Manage AI Models";
        n.ErrorText = "displaying model's main menu";
        n.Help = "Register, update, or remove models from a specific LLM provider.";
        n.AddCommand<ListModels>();
        n.AddCommand<AddModel>();
        n.AddCommand<UpdateModel>();
        n.AddCommand<RemoveModel>();
        n.AddCommand<ViewModel>();
        n.AddCommand<HelpCommand>();
        n.AddCommand<BackCommand>();
        n.AddCommand<ExitCommand>();
    }) {
    protected override async Task<Result> HandleCommandAsync(CancellationToken ct = default) {
        Logger.LogInformation("Showing Models main menu...");
        var choice = await Input.BuildSelectionPrompt("What would you like to do?")
                                .DisplayAs(MapTo)
                                .AddChoices(Commands.ToArray(c => c.Name))
                                .ShowAsync(ct);

        var command = Commands.FirstOrDefault(i => i.Name == choice);
        return command is null
            ? Result.Success()
            : await command.Execute([], ct);

        string MapTo(string item) => Commands.FirstOrDefault(i => i.Name == item)?.Description ?? string.Empty;
    }
}
