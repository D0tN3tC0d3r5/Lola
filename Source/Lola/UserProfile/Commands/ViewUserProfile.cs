namespace Lola.UserProfile.Commands;

public class ViewUserProfile(IHasChildren parent, IUserProfileHandler handler)
    : LolaCommand<ViewUserProfile>(parent, "Info", n => {
        n.Aliases = ["i"];
        n.ErrorText = "displaying the user profile";
        n.Description = "Display the User Profile.";
    }) {
    protected override Result HandleCommand() {
        Logger.LogInformation("Executing UserProfile->View command...");
        var user = handler.CurrentUser;
        if (user is null) {
            Logger.LogInformation("No user selected.");
            return Result.Success();
        }

        ShowDetails(user);
        return Result.Success();
    }

    private void ShowDetails(UserProfileEntity user) {
        Output.WriteLine("[yellow]User Information:[/]");
        Output.WriteLine($"[blue]Name:[/] {user.Name}");
    }
}
