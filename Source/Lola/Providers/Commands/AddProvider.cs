using Task = System.Threading.Tasks.Task;

namespace Lola.Providers.Commands;

public class AddProvider(IHasChildren parent, IProviderHandler handler)
    : LolaCommand<AddProvider>(parent, "Add", n => {
        n.Aliases = ["new"];
        n.Description = "Add a provider";
        n.ErrorText = "adding the new provider";
        n.Help = "Register a new LLM provider to use with your AI agents.";
    }) {
    protected override async Task<Result> HandleCommandAsync(CancellationToken ct = default) {
        Logger.LogInformation("Executing Providers->Add command...");
        var provider = new ProviderEntity();
        await SetUpAsync(provider, ct);
        handler.Add(provider);
        Output.WriteLine($"[green]Provider '{provider.Name}' added successfully.[/]");
        Logger.LogInformation("Provider '{ProviderId}:{ProviderName}' added successfully.", provider.Id, provider.Name);
        return Result.Success();
    }

    private async Task SetUpAsync(ProviderEntity provider, CancellationToken ct) {
        provider.Name = await Input.BuildMultilinePrompt("What is the name of the LLM provider?")
                                   .AsSingleLine()
                                   .AddValidation(n => ProviderEntity.ValidateName(null, n, handler))
                                   .ShowAsync(ct);

        if (await Input.ConfirmAsync("Do you have an API Key for this provider?", ct)) {
            provider.ApiKey = await Input.BuildMultilinePrompt("API Key:")
                                         .AsSingleLine()
                                         .AddValidation(n => ProviderEntity.ValidateApiKey(null, n, handler))
                                         .ShowAsync(ct);

            if (!string.IsNullOrWhiteSpace(provider.ApiKey))
                provider.IsEnabled = await Input.ConfirmAsync("Do you want to enable this provider now?", ct);
        }
    }
}
