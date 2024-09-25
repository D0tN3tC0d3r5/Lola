using Task = System.Threading.Tasks.Task;

namespace Lola.Providers.Commands;

public class UpdateProvider(IHasChildren parent, IProviderHandler handler)
    : LolaCommand<UpdateProvider>(parent, "Update", n => {
        n.Aliases = ["edit"];
        n.Description = "Update a provider";
        n.ErrorText = "updating the LLM provider";
        n.Help = "Update a LLM provider.";
    }) {
    protected override async Task<Result> HandleCommandAsync(CancellationToken ct = default) {
        Logger.LogInformation("Executing Providers->Update command...");
        var providers = handler.List();
        if (providers.Length == 0) {
            Output.WriteLine("[yellow]No providers found.[/]");
            Logger.LogInformation("No providers found. Remove provider action cancelled.");
            return Result.Success();
        }
        var provider = await this.SelectEntityAsync<ProviderEntity, uint>(providers.OrderBy(p => p.Name), m => m.Name, ct);
        if (provider is null) {
            Logger.LogInformation("Provider updated action cancelled.");
            return Result.Success();
        }
        await SetUpAsync(provider, ct);

        handler.Update(provider);
        Output.WriteLine($"[green]Provider '{provider.Name}' updated successfully.[/]");
        Logger.LogInformation("Provider '{ProviderId}:{ProviderName}' updated successfully.", provider.Id, provider.Name);
        return Result.Success();
    }

    private async Task SetUpAsync(ProviderEntity provider, CancellationToken ct) {
        provider.Name = await Input.BuildMultilinePrompt("Enter the new name for the provider")
                                   .WithDefault(provider.Name)
                                   .AsSingleLine()
                                   .AddValidation(name => ProviderEntity.ValidateName(provider.Id, name, handler))
                                   .ShowAsync(ct);

        if (string.IsNullOrWhiteSpace(provider.ApiKey)) {
            if (await Input.ConfirmAsync("Do you have an API Key for this provider?", ct)) {
                provider.ApiKey = await Input.BuildMultilinePrompt("API Key:")
                                             .AsSingleLine()
                                             .AddValidation(ProviderEntity.ValidateApiKey)
                                             .ShowAsync(ct);
            }
            provider.IsEnabled = false;
            return;
        }

        if (await Input.ConfirmAsync("Do you want to change or remove the API Key?", ct)) {
            provider.ApiKey = await Input.BuildMultilinePrompt("New API Key [yellow](clear the value to remove it)[/]:")
                                         .AsSingleLine()
                                         .WithDefault(provider.ApiKey)
                                         .ShowAsync(ct);
        }

        if (!string.IsNullOrWhiteSpace(provider.ApiKey)) {
            var message = provider.IsEnabled
                              ? "Do you want to keep this provider enabled?"
                              : "Do you want to activate this provider?";
            provider.IsEnabled = await Input.ConfirmAsync(message, ct);
            return;
        }

        if (provider.IsEnabled)
            provider.IsEnabled = false;
    }
}
