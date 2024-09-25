namespace Lola.Providers.Commands;

public class ProvidersMainMenu(IHasChildren parent)
    : LolaCommand<ProvidersMainMenu>(parent, "Providers", n => {
        n.Description = "Manage LLM Providers";
        n.Help = "Register, update, or remove LLM providers.";
        n.ErrorText = "displaying provider's main menu";
        n.AddCommand<ListProviders>();
        n.AddCommand<AddProvider>();
        n.AddCommand<ViewProvider>();
        n.AddCommand<UpdateProvider>();
        n.AddCommand<RemoveProvider>();
        n.AddCommand<HelpCommand>();
        n.AddCommand<BackCommand>();
        n.AddCommand<ExitCommand>();
    }) {
    protected override async Task<Result> HandleCommandAsync(CancellationToken ct = default) {
        Logger.LogInformation("Showing Providers main menu...");
        var choice = await Input.BuildSelectionPrompt<string>("What would you like to do?")
                                .ConvertWith(MapTo)
                                .AddChoices(Commands.ToArray(c => c.Name))
                                .ShowAsync(ct);

        var command = Commands.FirstOrDefault(i => i.Name == choice);
        return command is null
            ? Result.Success()
            : await command.Execute([], ct);

        string MapTo(string item) => Commands.FirstOrDefault(i => i.Name == item)?.Description ?? string.Empty;
    }
}
