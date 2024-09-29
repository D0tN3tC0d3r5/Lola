namespace Lola.Models.Commands;

public class ListModels(IHasChildren parent, IModelHandler modelHandler, IProviderHandler providerHandler)
    : LolaCommand<ListModels>(parent, "List", n => {
        n.Aliases = ["ls"];
        n.Description = "List models";
        n.ErrorText = "listing the existing models";
        n.Help = "List all the models or those from a specific LLM provider.";
    }) {
    protected override async Task<Result> HandleCommandAsync(CancellationToken ct = default) {
        Logger.LogInformation("Executing Models->List command...");
        var providers = providerHandler.List();
        if (providers.Length == 0) {
            Output.WriteLine("[yellow bold]No providers available. Please add a provider first.[/]");
            Logger.LogInformation("No providers available. List models action cancelled.");
            return Result.Invalid("No providers available.");
        }
        var selectedChoice = await Input.BuildSelectionPrompt<ProviderEntity>("Select a provider:", p => p.Id)
                                    .DisplayAs(p => p.Name)
                                    .AddChoice(0, "Display all models", ChoicePosition.AtStart)
                                    .AddChoices(providers)
                                    .ShowAsync(ct);
        var models = selectedChoice is null
            ? modelHandler.List()
            : modelHandler.List(m => m.Id == selectedChoice.Id);
        if (models.Length == 0) {
            Output.WriteLine("[yellow]No models found.[/]");
            return Result.Success();
        }

        var sortedList = models.OrderBy(m => m.Provider!.Name).ThenBy(m => m.Name);

        ShowList(sortedList);

        return Result.Success();
    }

    private void ShowList(IOrderedEnumerable<ModelEntity> sortedModels) {
        var table = new Table();
        table.Expand();
        table.AddColumn(new("[yellow]Name[/]"));
        table.AddColumn(new("[yellow]Provider[/]"));
        table.AddColumn(new("[yellow]Id[/]"));
        table.AddColumn(new TableColumn("[yellow]Map Size[/]").RightAligned());
        table.AddColumn(new TableColumn("[yellow]Output Tokens[/]").RightAligned());
        foreach (var model in sortedModels) {
            var provider = providerHandler.Find(p => p.Id == model.ProviderId);
            table.AddRow(
                model.Name,
                provider?.Name ?? "Undefined",
                model.Key,
                $"{model.MaximumContextSize:#,##0}",
                $"{model.MaximumOutputTokens:#,##0}"
            );
        }
        Output.Write(table);
    }
}
