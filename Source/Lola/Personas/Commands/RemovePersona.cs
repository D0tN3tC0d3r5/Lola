namespace Lola.Personas.Commands;

public class RemovePersona(IHasChildren parent, IPersonaHandler handler)
    : LolaCommand<RemovePersona>(parent, "Remove", n => {
        n.Aliases = ["delete", "del"];
        n.Description = "Remove a persona";
        n.ErrorText = "removing the persona";
        n.Help = "Remove an existing agent's persona.";
    }) {
    protected override async Task<Result> HandleCommandAsync(CancellationToken ct = default) {
        Logger.LogInformation("Executing Personas->Remove command...");
        var personas = handler.List();
        if (personas.Length == 0) {
            Output.WriteLine("[yellow]No personas found.[/]");
            Logger.LogInformation("No personas found. Remove persona action cancelled.");
            return Result.Success();
        }
        var persona = await this.SelectEntityAsync<PersonaEntity, uint>(personas.OrderBy(p => p.Name), p => p.Name, ct);
        if (persona is null) {
            Logger.LogInformation("No persona selected.");
            return Result.Success();
        }

        if (!await Input.ConfirmAsync($"Are you sure you want to remove the persona '{persona.Name}' ({persona.Id})?", ct)) {
            Logger.LogInformation("Persona removal cancelled by user.");
            return Result.Invalid("Action cancelled.");
        }

        handler.Remove(persona.Id);
        Output.WriteLine($"[green]Persona '{persona.Name}' removed successfully.[/]");
        Logger.LogInformation("Persona '{PersonaId}:{PersonaName}' removed successfully.", persona.Id, persona.Name);
        return Result.Success();
    }
}
