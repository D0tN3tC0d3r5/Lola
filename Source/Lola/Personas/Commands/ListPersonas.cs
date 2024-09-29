namespace Lola.Personas.Commands;

public class ListPersonas(IHasChildren parent, IPersonaHandler personaHandler)
    : LolaCommand<ListPersonas>(parent, "List", n => {
        n.Aliases = ["ls"];
        n.Description = "List existing personas";
        n.ErrorText = "listing personas";
        n.Help = "List all the existing agent's personas.";
    }) {
    protected override Result HandleCommand() {
        Logger.LogInformation("Executing Personas->List command...");
        var personas = personaHandler.List();

        if (personas.Length == 0) {
            Output.WriteLine("[yellow]No personas found.[/]");
            Output.WriteLine();

            return Result.Success();
        }

        var sortedList = personas.OrderBy(p => p.Name);
        ShowList(sortedList);

        return Result.Success();
    }

    private void ShowList(IEnumerable<PersonaEntity> personas) {
        var sortedPersonas = personas.OrderBy(m => m.Name);
        var table = new Table();
        table.Expand();
        table.AddColumn(new("[yellow]Name[/]"));
        table.AddColumn(new("[yellow]Role[/]"));
        table.AddColumn(new("[yellow]Main Objective[/]"));
        foreach (var persona in sortedPersonas)
            table.AddRow(persona.Name, persona.Role, persona.Objectives.FirstOrDefault() ?? "[red][Undefined][/]");
        Output.Write(table);
    }
}
