namespace Lola.UserProfile.Repositories;

public class UserProfileStorage(IConfiguration configuration)
    : JsonFilePerTypeStorage<UserProfileEntity>("profile", configuration),
      IUserProfileStorage;
