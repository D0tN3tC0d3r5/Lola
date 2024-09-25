namespace Lola.Models.Commands;

public class SelectDefaultModel(IHasChildren parent, IModelHandler handler)
    : LolaCommand<SelectDefaultModel>(parent, "Select", n => {
        n.Aliases = ["sel"];
        n.Description = "Select default model.";
        n.ErrorText = "selecting the default model";
        n.Help = "Select the main model used by the app.";
    }) {
    protected override async Task<Result> HandleCommandAsync(CancellationToken ct = default) {
        Logger.LogInformation("Executing Models->Select default model command...");
        var models = handler.List();
        if (models.Length == 0) {
            Output.WriteLine("[yellow]No models available. Please add a model before proceeding.[/]");
            return Result.Success();
        }

        var selected = await Input.BuildSelectionPrompt<ModelEntity>("Select an model:")
                                  .AddChoices(models.OrderBy(m => m.ProviderId).ThenBy(m => m.Name))
                                  .ConvertWith(c => c.Name)
                                  .ShowAsync(ct);

        handler.Select(selected.Id);
        Output.WriteLine($"[green]Settings '{selected.Key}' selected successfully.[/]");
        return Result.Success();
    }
}
