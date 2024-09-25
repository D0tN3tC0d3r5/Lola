namespace Lola.UserProfile.Handlers;

public class UserProfileHandler(IUserProfileDataSource dataSource, ILogger<UserProfileHandler> logger)
    : IUserProfileHandler {
    private UserProfileEntity? _currentUser;

    public UserProfileEntity Create(Action<UserProfileEntity>? setUp = null)
        => dataSource.Create(setUp);
    public UserProfileEntity? CurrentUser
        => _currentUser ??= dataSource.FirstOrDefault(i => !i.Internal);
    public void Set(UserProfileEntity user) {
        if (dataSource.Any()) dataSource.Update(user);
        else dataSource.Add(user);
        logger.LogInformation("User profile set.");
    }
}
