using Task = System.Threading.Tasks.Task;
using ValidationException = DotNetToolbox.Results.ValidationException;

namespace Lola.Personas.Commands;

public class PersonaUpdateCommand(IHasChildren parent, IPersonaHandler handler)
    : Command<PersonaUpdateCommand>(parent, "Update", n => {
        n.Aliases = ["edit"];
        n.Description = "Update a persona";
        n.Help = "Update an existing agent's persona.";
    }) {
    protected override Task<Result> ExecuteAsync(CancellationToken ct = default) => this.HandleCommandAsync(async lt => {
        try {
            Logger.LogInformation("Executing Personas->Update command...");
            var personas = handler.List();
            if (personas.Length == 0) {
                Output.WriteLine("[yellow]No personas found.[/]");
                Logger.LogInformation("No personas found. Update persona action cancelled.");
                return Result.Success();
            }
            var persona = await this.SelectEntityAsync<PersonaEntity, uint>(personas.OrderBy(p => p.Name), p => p.Name, lt);
            if (persona is null) {
                Logger.LogInformation("No persona selected.");
                return Result.Success();
            }

            await SetUpAsync(persona, lt);

            handler.Update(persona);
            Output.WriteLine($"[green]Persona '{persona.Name}' updated successfully.[/]");
            Logger.LogInformation("Persona '{PersonaKey}:{PersonaName}' updated successfully.", persona.Key, persona.Name);
            return Result.Success();
        }
        catch (ValidationException ex) {
            var errors = string.Join("\n", ex.Errors.Select(e => $" - {e.Source}: {e.Message}"));
            Logger.LogWarning("Error updating the persona. Validation errors:\n{Errors}", errors);
            Output.WriteLine($"[red]We found some problems while updating the persona. Please correct the following errors and try again:\n{errors}[/]");
            return Result.Invalid(ex.Errors);
        }
    }, "Error updating the persona.", ct);

    private async Task SetUpAsync(PersonaEntity persona, CancellationToken ct) {
        // Update Name
        persona.Name = await Input.BuildTextPrompt<string>($"Enter the new name for the persona (or press Enter to keep current):")
                                  .WithDefault(persona.Name)
                                  .AsRequired()
                                  .AddValidation(name => PersonaEntity.ValidateName(name, handler))
                                  .ShowAsync(ct);

        // Update Role
        persona.Role = await Input.BuildTextPrompt<string>($"Enter the new role for the persona (or press Enter to keep current):")
                                  .WithDefault(persona.Role)
                                  .AsRequired()
                                  .AddValidation(PersonaEntity.ValidateRole)
                                  .ShowAsync(ct);

        // Update Goals
        Output.WriteLine($"This persone has {persona.Goals.Count} goals.");
        for (var i = 0; i < persona.Goals.Count; i++) {
            Output.WriteLine($"- Goal {i + 1}:");
            Output.WriteLine(persona.Goals[i]);
            var goalAction = await Input.BuildTextPrompt<string>("Would you like to change this goal?")
                                        .AddChoices(["Keep", "Change", "Remove"])
                                        .WithDefault("Keep")
                                        .ShowAsync(ct);
            if (goalAction == "Remove") persona.Goals[i] = "[Remove]";
            if (goalAction is "Keep" or "Remove") continue;
            persona.Goals[i] = await Input.BuildMultilinePrompt("What is the updated goal?")
                                          .Validate(PersonaEntity.ValidateGoal)
                                          .ShowAsync(ct);
        }
        foreach (var goalToRemove in persona.Goals.Where(i => i == "[Remove]"))
            persona.Goals.Remove(goalToRemove);

        // Update Expertise
        var newExpertise = await Input.BuildTextPrompt<string>($"Enter the new expertise for the persona (or press Enter to keep current):")
                                      .AsRequired()
                                      .WithDefault(persona.Expertise)
                                      .ShowAsync(ct);
        if (!string.IsNullOrEmpty(newExpertise)) {
            persona.Expertise = newExpertise;
        }

        // Update Traits
        Output.WriteLine("Current Traits:");
        foreach (var trait in persona.Traits) {
            Output.WriteLine($" - {trait}");
        }
        var updateTraits = await Input.ConfirmAsync("Would you like to update the traits?", ct);
        if (updateTraits) {
            persona.Traits.Clear();
            var traits = await Input.BuildMultilinePrompt("Enter the new traits for the persona (separate multiple traits with new lines):")
                                    .ShowAsync(ct);
            persona.Traits.AddRange(traits.Replace("\r", "").Split("\n"));
        }

        // Update Important
        Output.WriteLine("Current Important Requirements:");
        foreach (var important in persona.Important) {
            Output.WriteLine($" - {important}");
        }
        var updateImportant = await Input.ConfirmAsync("Would you like to update the important requirements?", ct);
        if (updateImportant) {
            persona.Important.Clear();
            var importantItems = await Input.BuildMultilinePrompt("Enter the new important requirements (separate multiple items with new lines):")
                                            .ShowAsync(ct);
            persona.Important.AddRange(importantItems.Replace("\r", "").Split("\n"));
        }

        // Update Negative
        Output.WriteLine("Current Negative Restrictions:");
        foreach (var negative in persona.Negative) {
            Output.WriteLine($" - {negative}");
        }
        var updateNegative = await Input.ConfirmAsync("Would you like to update the negative restrictions?", ct);
        if (updateNegative) {
            persona.Negative.Clear();
            var negativeItems = await Input.BuildMultilinePrompt("Enter the new negative restrictions (separate multiple items with new lines):")
                                           .ShowAsync(ct);
            persona.Negative.AddRange(negativeItems.Replace("\r", "").Split("\n"));
        }

        // Update Other
        Output.WriteLine("Current Other Information:");
        foreach (var other in persona.Other) {
            Output.WriteLine($" - {other}");
        }
        var updateOther = await Input.ConfirmAsync("Would you like to update the other information?", ct);
        if (updateOther) {
            persona.Other.Clear();
            var otherItems = await Input.BuildMultilinePrompt("Enter the new other information (separate multiple items with new lines):")
                                        .ShowAsync(ct);
            persona.Other.AddRange(otherItems.Replace("\r", "").Split("\n"));
        }
    }
}
