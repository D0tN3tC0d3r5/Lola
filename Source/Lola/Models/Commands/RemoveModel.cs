﻿namespace Lola.Models.Commands;

public class RemoveModel(IHasChildren parent, IModelHandler handler)
    : LolaCommand<RemoveModel>(parent, "Remove", n => {
        n.Aliases = ["delete", "del"];
        n.Description = "Remove a model";
        n.ErrorText = "removing a model";
        n.Help = "Remove a model.";
    }) {
    protected override async Task<Result> HandleCommandAsync(CancellationToken ct = default) {
        Logger.LogInformation("Executing Models->Remove command...");
        var models = handler.List();
        if (models.Length == 0) {
            Output.WriteLine("[yellow]No models found.[/]");
            Logger.LogInformation("No models found. Remove model action cancelled.");
            return Result.Success();
        }
        var model = await this.SelectEntityAsync<ModelEntity, uint>(models.OrderBy(m => m.ProviderId).ThenBy(m => m.Name), m => m.Name, ct);
        if (model is null) {
            Logger.LogInformation("No model selected.");
            return Result.Success();
        }

        if (!await Input.ConfirmAsync($"Are you sure you want to remove the model '{model.Name}' ({model.Key})?", ct)) {
            return Result.Invalid("Action cancelled.");
        }

        handler.Remove(model.Id);
        Output.WriteLine($"[green]Settings with key '{model.Name}' removed successfully.[/]");
        return Result.Success();
    }
}
