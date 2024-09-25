namespace Lola.Providers.Commands;

public class ListProviders(IHasChildren parent, IProviderHandler providerHandler)
    : LolaCommand<ListProviders>(parent, "List", n => {
        n.Aliases = ["ls"];
        n.Description = "List all providers";
        n.ErrorText = "listing the existing providers";
        n.Help = "List all LLM providers.";
    }) {
    protected override Result HandleCommand() {
        Logger.LogInformation("Executing Providers->List command...");
        var providers = providerHandler.List();
        if (providers.Length == 0) {
            Output.WriteLine("[yellow]No providers found.[/]");
            Logger.LogInformation("No providers found. List providers action cancelled.");
            return Result.Success();
        }

        var sortedList = providers.OrderBy(p => p.Name);
        ShowList(sortedList);

        Logger.LogInformation("Providers listed.");
        return Result.Success();
    }

    private void ShowList(IEnumerable<ProviderEntity> providers) {
        var table = new Table();
        table.AddColumn(new("[yellow]Id[/]"));
        table.AddColumn(new("[yellow]Name[/]"));
        foreach (var provider in providers) {
            table.AddRow(provider.Id.ToString(), provider.Name);
        }

        Output.Write(table);
    }
}
