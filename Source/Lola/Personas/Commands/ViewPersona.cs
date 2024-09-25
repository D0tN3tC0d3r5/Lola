namespace Lola.Personas.Commands;

public class ViewPersona(IHasChildren parent, IPersonaHandler handler)
    : LolaCommand<ViewPersona>(parent, "Info", n => {
        n.Aliases = ["i"];
        n.Description = "View persona details";
        n.ErrorText = "displaying the persona information";
        n.Help = "Display detailed information about an agent's persona.";
    }) {
    protected override async Task<Result> HandleCommandAsync(CancellationToken ct = default) {
        Logger.LogInformation("Executing Personas->Info command...");
        var personas = handler.List();
        if (personas.Length == 0) {
            Output.WriteLine("[yellow]No personas found.[/]");
            Logger.LogInformation("No personas found. View persona action cancelled.");
            return Result.Success();
        }
        var persona = await this.SelectEntityAsync<PersonaEntity, uint>(personas.OrderBy(p => p.Name), p => p.Name, ct);
        if (persona is null) {
            Logger.LogInformation("No persona selected.");
            return Result.Success();
        }

        ShowDetails(persona);
        return Result.Success();
    }

    private void ShowDetails(PersonaEntity persona) {
        Output.WriteLine($"{persona.Name} [yellow]Information:[/]");
        Output.WriteLine();
        Output.WriteLine($"[blue]{nameof(Persona.Role)}:[/]");
        Output.WriteLine(persona.Role);
        Output.WriteLine();
        Output.WriteLine($"[blue]{nameof(Persona.Expertise)}:[/]");
        Output.WriteLine(persona.Expertise);
        Output.WriteLine();
        Output.WriteLine($"[blue]{nameof(Persona.Goals)}:[/]");
        foreach (var goal in persona.Goals) Output.WriteLine($" - {goal}");
        Output.WriteLine();
        Output.WriteLine($"[blue]{nameof(Persona.Characteristics)}:[/]");
        foreach (var characteristic in persona.Characteristics) Output.WriteLine($" - {characteristic}");
        Output.WriteLine();
        Output.WriteLine($"[blue]{nameof(Persona.Requirements)}:[/]");
        foreach (var requirement in persona.Requirements) Output.WriteLine($" - {requirement}");
        Output.WriteLine();
        Output.WriteLine($"[blue]{nameof(Persona.Restrictions)}:[/]");
        foreach (var restriction in persona.Restrictions) Output.WriteLine($" - {restriction}");
        Output.WriteLine();
        Output.WriteLine($"[blue]{nameof(Persona.Traits)}:[/]");
        foreach (var trait in persona.Traits) Output.WriteLine($" - {trait}");
        Output.WriteLine();
    }
}
