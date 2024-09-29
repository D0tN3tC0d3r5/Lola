namespace Lola.Models.Commands;

public class ViewModel(IHasChildren parent, IModelHandler handler)
    : LolaCommand<ViewModel>(parent, "Info", n => {
        n.Aliases = ["i"];
        n.Description = "View model";
        n.ErrorText = "displaying the model information";
        n.Help = "Display detailed information about a model.";
    }) {
    protected override async Task<Result> HandleCommandAsync(CancellationToken ct = default) {
        Logger.LogInformation("Executing Models->Info command...");
        var models = handler.List();
        if (models.Length == 0) {
            Output.WriteLine("[yellow]No models found.[/]");
            Logger.LogInformation("No models found. View model action cancelled.");
            return Result.Success();
        }
        var model = await this.SelectEntityAsync<ModelEntity, uint>(models.OrderBy(m => m.ProviderId).ThenBy(m => m.Name), m => m.Name, ct);
        if (model is null) {
            Logger.LogInformation("No model selected.");
            return Result.Success();
        }
        handler.IncludeProvider(model);

        ShowDetails(model);

        return Result.Success();
    }

    private void ShowDetails(ModelEntity model) {
        Output.WriteLine("[yellow]Model Information:[/]");
        Output.WriteLine($"[blue]Id:[/] {model.Key}{(model.Selected ? " [green](default)[/]" : "")}");
        Output.WriteLine($"[blue]Name:[/] {model.Name}");
        Output.WriteLine($"[blue]Provider:[/] {model.Provider!.Name}");
        Output.WriteLine($"[blue]Maximum Map Size:[/] {model.MaximumContextSize}");
        Output.WriteLine($"[blue]Maximum Output Tokens:[/] {model.MaximumOutputTokens}");
        Output.WriteLine($"[blue]Input Cost per MTok:[/] {model.InputCostPerMillionTokens:C}");
        Output.WriteLine($"[blue]Output Cost per MTok:[/] {model.OutputCostPerMillionTokens:C}");
        Output.WriteLine($"[blue]Training Date Cut-Off:[/] {model.TrainingDateCutOff:MMM yyyy}");
        Output.WriteLine();
    }
}
