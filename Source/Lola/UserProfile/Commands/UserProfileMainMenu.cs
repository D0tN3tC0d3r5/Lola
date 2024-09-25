namespace Lola.UserProfile.Commands;

public class UserProfileMainMenu(IHasChildren parent)
    : LolaCommand<UserProfileMainMenu>(parent, "UserProfile", n => {
        n.Description = "Manage Your Profile";
        n.ErrorText = "displaying the user profile main menu";
        n.AddCommand<UpdateUserProfile>();
        n.AddCommand<HelpCommand>();
    }) {
    protected override async Task<Result> HandleCommandAsync(CancellationToken ct = default) {
        Logger.LogInformation("Executing UserProfile command...");
        var choice = await Input.BuildSelectionPrompt<string>("What would you like to do?")
                                .ConvertWith(MapTo)
                                .AddChoices(Commands.ToArray(c => c.Name))
                                .ShowAsync(ct);

        var command = Commands.FirstOrDefault(i => i.Name == choice);
        return command is null
            ? Result.Success()
            : await command.Execute([], ct);

        string MapTo(string item) => Commands.FirstOrDefault(i => i.Name == item)?.Description ?? string.Empty;
    }
}
