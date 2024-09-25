using Task = System.Threading.Tasks.Task;

namespace Lola.Personas.Commands;

public class UpdatePersona(IHasChildren parent, IPersonaHandler handler)
    : LolaCommand<UpdatePersona>(parent, "Update", n => {
        n.Aliases = ["edit"];
        n.Description = "Update a persona";
        n.ErrorText = "updating the persona";
        n.Help = "Update an existing agent's persona.";
    }) {
    protected override async Task<Result> HandleCommandAsync(CancellationToken ct = default) {
        Logger.LogInformation("Executing Personas->Update command...");
        var personas = handler.List();
        if (personas.Length == 0) {
            Output.WriteLine("[yellow]No personas found.[/]");
            Logger.LogInformation("No personas found. Update persona action cancelled.");
            return Result.Success();
        }
        var persona = await this.SelectEntityAsync<PersonaEntity, uint>(personas.OrderBy(p => p.Name), p => p.Name, ct);
        if (persona is null) {
            Logger.LogInformation("No persona selected.");
            return Result.Success();
        }

        await SetUpAsync(persona, ct);

        handler.Update(persona);
        Output.WriteLine($"[green]Persona '{persona.Name}' updated successfully.[/]");
        Logger.LogInformation("Persona '{PersonaId}:{PersonaName}' updated successfully.", persona.Id, persona.Name);
        return Result.Success();
    }

    private async Task SetUpAsync(PersonaEntity persona, CancellationToken ct) {
        // Update Name
        persona.Name = await Input.BuildTextPrompt<string>("- Name (ENTER to keep current):")
                                  .WithDefault(persona.Name)
                                  .ShowOptionalFlag()
                                  .AddValidation(name => PersonaEntity.ValidateName(persona.Id, name, handler))
                                  .ShowAsync(ct);

        // Update Role
        persona.Role = await Input.BuildTextPrompt<string>("- Role (ENTER to keep current):")
                                  .WithDefault(persona.Role)
                                  .ShowOptionalFlag()
                                  .AddValidation(PersonaEntity.ValidateRole)
                                  .ShowAsync(ct);

        // Update Goals
        Output.WriteLine($"This persona has currently {persona.Goals.Count} goals.");
        var goalCount = 0;
        while (goalCount < persona.Goals.Count) {
            persona.Goals[goalCount] = await Input.BuildMultilinePrompt($"- Goal {goalCount + 1}:")
                                                  .WithDefault(persona.Goals[goalCount])
                                                  .AddValidation(PersonaEntity.ValidateGoal)
                                                  .ShowAsync(ct);
            goalCount++;
        }
        var addGoal = await Input.ConfirmAsync("Would you like to add another goal?", ct);
        while (addGoal) {
            persona.Goals[goalCount] = await Input.BuildMultilinePrompt($"- Goal {goalCount + 1}:")
                                                  .AddValidation(PersonaEntity.ValidateGoal)
                                                  .ShowAsync(ct);
            addGoal = await Input.ConfirmAsync("Would you like to add another goal?", ct);
        }

        // Update Expertise
        persona.Expertise = await Input.BuildMultilinePrompt("- Agent Expertise:")
                                       .WithDefault(persona.Expertise)
                                       .ShowAsync(ct);

        // Update traits
        Output.WriteLine($"This persona has currently {persona.Characteristics.Count} characteristics.");
        var characteristicCount = 0;
        while (characteristicCount < persona.Characteristics.Count) {
            persona.Characteristics[characteristicCount] = await Input.BuildMultilinePrompt($"- Characteristic {characteristicCount + 1}:")
                                                   .WithDefault(persona.Characteristics[characteristicCount])
                                                   .AddValidation(PersonaEntity.ValidateCharacteristic)
                                                   .ShowAsync(ct);
            characteristicCount++;
        }
        var addCharacteristic = await Input.ConfirmAsync("Would you like to add another characteristic?", ct);
        while (addCharacteristic) {
            persona.Characteristics[characteristicCount] = await Input.BuildMultilinePrompt($"- Characteristic {characteristicCount + 1}:")
                                                   .AddValidation(PersonaEntity.ValidateCharacteristic)
                                                   .ShowAsync(ct);
            addCharacteristic = await Input.ConfirmAsync("Would you like to add another characteristic?", ct);
        }

        // Update Requirements
        Output.WriteLine($"This persona has currently {persona.Requirements.Count} requirements.");
        var requirementCount = 0;
        while (requirementCount < persona.Requirements.Count) {
            persona.Requirements[requirementCount] = await Input.BuildMultilinePrompt($"- Requirement {requirementCount + 1}:")
                                                   .WithDefault(persona.Requirements[requirementCount])
                                                   .AddValidation(PersonaEntity.ValidateRequirement)
                                                   .ShowAsync(ct);
            requirementCount++;
        }
        var addRequirement = await Input.ConfirmAsync("Would you like to add another requirement?", ct);
        while (addRequirement) {
            persona.Requirements[requirementCount] = await Input.BuildMultilinePrompt($"- Requirement {requirementCount + 1}:")
                                                   .AddValidation(PersonaEntity.ValidateRequirement)
                                                   .ShowAsync(ct);
            addRequirement = await Input.ConfirmAsync("Would you like to add another requirement?", ct);
        }

        // Update Restrictions
        Output.WriteLine($"This persona has currently {persona.Restrictions.Count} restrictions.");
        var restrictionCount = 0;
        while (restrictionCount < persona.Restrictions.Count) {
            persona.Restrictions[restrictionCount] = await Input.BuildMultilinePrompt($"- Restriction {restrictionCount + 1}:")
                                                         .WithDefault(persona.Restrictions[restrictionCount])
                                                         .AddValidation(PersonaEntity.ValidateRestriction)
                                                         .ShowAsync(ct);
            restrictionCount++;
        }
        var addRestriction = await Input.ConfirmAsync("Would you like to add another restriction?", ct);
        while (addRestriction) {
            persona.Restrictions[restrictionCount] = await Input.BuildMultilinePrompt($"- Restriction {restrictionCount + 1}:")
                                                         .AddValidation(PersonaEntity.ValidateRestriction)
                                                         .ShowAsync(ct);
            addRestriction = await Input.ConfirmAsync("Would you like to add another restriction?", ct);
        }

        // Update Traits
        Output.WriteLine($"This persona has currently {persona.Traits.Count} traits.");
        var traitCount = 0;
        while (traitCount < persona.Traits.Count) {
            persona.Traits[traitCount] = await Input.BuildMultilinePrompt($"- Traits {traitCount + 1}:")
                                                                .WithDefault(persona.Traits[traitCount])
                                                                .AddValidation(PersonaEntity.ValidateTrait)
                                                                .ShowAsync(ct);
            traitCount++;
        }
        var addTrait = await Input.ConfirmAsync("Would you like to add another trait?", ct);
        while (addTrait) {
            persona.Traits[traitCount] = await Input.BuildMultilinePrompt($"- Traits {traitCount + 1}:")
                                                                .AddValidation(PersonaEntity.ValidateTrait)
                                                                .ShowAsync(ct);
            addTrait = await Input.ConfirmAsync("Would you like to add another trait?", ct);
        }
    }
}
