using AIUserProfile = DotNetToolbox.AI.Jobs.UserProfile;

namespace Lola.UserProfile.Repositories;

public class UserProfileEntityTests {
    [Fact]
    public void Validate_WithValidEntity_ShouldReturnSuccess() {
        // Arrange
        var entity = new UserProfileEntity {
            Id = 1,
            Name = "Test User",
        };

        // Act
        var result = entity.Validate();

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithInvalidName_ShouldReturnError(string? name) {
        // Arrange
        var entity = new UserProfileEntity {
            Id = 1,
            Name = name!,
        };

        // Act
        var result = entity.Validate();

        // Assert
        result.IsInvalid.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "The name cannot be blank.");
    }

    [Fact]
    public void ValidateName_WithValidName_ShouldReturnSuccess() {
        // Arrange
        const string name = "Valid Name";

        // Act
        var result = UserProfileEntity.ValidateName(name);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ValidateName_WithInvalidName_ShouldReturnError(string? name) {
        // Act
        var result = UserProfileEntity.ValidateName(name);

        // Assert
        result.IsInvalid.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "The name cannot be blank.");
    }

    [Fact]
    public void ImplicitConversion_ToAIUserProfile_ShouldConvertCorrectly() {
        // Arrange
        var entity = new UserProfileEntity {
            Id = 1,
            Internal = true,
            Name = "Test User",
        };
        entity.Facts.Add("Fact 1");
        entity.Facts.Add("Fact 2");

        // Act
        AIUserProfile profile = entity;

        // Assert
        profile.Id.Should().Be(entity.Id);
        profile.Name.Should().Be(entity.Name);
        profile.Facts.Should().BeEquivalentTo(entity.Facts);
    }

    [Fact]
    public void Facts_ShouldBeInitializedAsEmptyList() {
        // Arrange & Act
        var entity = new UserProfileEntity();

        // Assert
        entity.Facts.Should().NotBeNull();
        entity.Facts.Should().BeEmpty();
    }
}
