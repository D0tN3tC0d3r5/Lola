﻿using Lola.Utilities;

namespace Lola.Personas.Commands;

public class PersonasCommand(IHasChildren parent)
    : Command<PersonasCommand>(parent, "Personas", n => {
        n.Description = "Manage AI Personas.";
        n.AddCommand<PersonaListCommand>();
        n.AddCommand<PersonaGenerateCommand>();
        //n.AddCommand<PersonaUpdateCommand>();
        //n.AddCommand<PersonaRemoveCommand>();
        //n.AddCommand<PersonaViewCommand>();
        n.AddCommand<HelpCommand>();
    }) {
    protected override Task<Result> ExecuteAsync(CancellationToken ct = default) => this.HandleCommandAsync(async lt => {
        Logger.LogInformation("Executing Personas->Main command...");
        var cts = CancellationTokenSource.CreateLinkedTokenSource(lt, ct);
        var choice = await Input.BuildSelectionPrompt<string>("What would you like to do?")
                                .ConvertWith(MapTo)
                                .AddChoices(Commands.ToArray(c => c.Name))
                                .ShowAsync(cts.Token);

        var command = Commands.FirstOrDefault(i => i.Name == choice);
        return command is null
                   ? Result.Success()
                   : await command.Execute([], cts.Token);

        string MapTo(string item) => Commands.FirstOrDefault(i => i.Name == item)?.Description ?? string.Empty;
    }, "Error displaying the persona menu.", ct);
}
