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
        var choices = providers.ToList(p => new ListItem<ProviderEntity, uint>(p.Id, p.Name, p));
        var cancelOption = new ListItem<ProviderEntity, uint>(0, "All", null);
        choices.Insert(0, cancelOption);
        var selectedChoice = await Input.BuildSelectionPrompt<ListItem<ProviderEntity, uint>>("Select a provider:")
                                    .ConvertWith(p => p.Text)
                                    .AddChoices([.. choices])
                                    .ShowAsync(ct);
        var models = selectedChoice.Key == default
            ? modelHandler.List()
            : modelHandler.List(selectedChoice.Item!.Id);
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
            var provider = providerHandler.GetById(model.ProviderId)!;
            table.AddRow(
                model.Name,
                provider.Name,
                model.Key,
                $"{model.MaximumContextSize:#,##0}",
                $"{model.MaximumOutputTokens:#,##0}"
            );
        }
        Output.Write(table);
    }
}
