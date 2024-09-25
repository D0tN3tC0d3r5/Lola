namespace Lola.Providers.Commands;

public class ViewProvider(IHasChildren parent, IProviderHandler handler)
    : LolaCommand<ViewProvider>(parent, "Info", n => {
        n.Aliases = ["i"];
        n.Description = "View a provider";
        n.ErrorText = "displaying the provider";
        n.Help = "Display the detailed information about a LLM Provider.";
    }) {
    protected override async Task<Result> HandleCommandAsync(CancellationToken ct = default) {
        Logger.LogInformation("Executing Providers->View command...");
        var providers = handler.List();
        if (providers.Length == 0) {
            Output.WriteLine("[yellow]No providers found.[/]");
            Logger.LogInformation("No providers found. View provider action cancelled.");
            return Result.Success();
        }

        var provider = await this.SelectEntityAsync<ProviderEntity, uint>(providers.OrderBy(p => p.Name), m => m.Name, ct);
        if (provider is null) {
            Logger.LogInformation("No provider selected.");
            return Result.Success();
        }

        ShowDetails(provider);
        return Result.Success();
    }

    private void ShowDetails(ProviderEntity provider) {
        Output.WriteLine("[yellow]Provider Information:[/]");
        Output.WriteLine($"[blue]Name:[/] {provider.Name}");
        Output.WriteLine($"[blue]API Id:[/] {provider.ApiKey ?? "[red]Not Set[/]"}");
    }
}
