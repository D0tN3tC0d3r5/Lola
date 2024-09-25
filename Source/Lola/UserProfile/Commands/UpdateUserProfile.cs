using Task = System.Threading.Tasks.Task;

namespace Lola.UserProfile.Commands;

public class UpdateUserProfile(IHasChildren parent, IUserProfileHandler handler)
    : LolaCommand<UpdateUserProfile>(parent, "Change", n => {
        n.Aliases = ["set"];
        n.ErrorText = "setting the user profile";
        n.Description = "Update your profile.";
    }) {
    protected override async Task<Result> HandleCommandAsync(CancellationToken ct = default) {
        Logger.LogInformation("Executing UserProfile->Set command...");
        var user = handler.CurrentUser ?? handler.Create();
        await SetUpAsync(user, ct);
        handler.Set(user);

        Output.WriteLine("[green]User profile set successfully.[/]");
        Logger.LogInformation("User profile set successfully.");
        return Result.Success();
    }

    private async Task SetUpAsync(UserProfileEntity user, CancellationToken ct)
        => user.Name = await Input.BuildTextPrompt<string>("How would you like me to call you?")
                                  .AddValidation(UserProfileEntity.ValidateName)
                                  .ShowAsync(ct);
}
