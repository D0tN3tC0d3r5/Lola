namespace Lola.Goals.Repositories;

public class GoalDataSourceTests {
    [Fact]
    public void Constructor_ShouldInitializeCorrectly() {
        // Arrange
        var mockStorage = Substitute.For<IGoalStorage>();

        // Act
        var subject = new GoalDataSource(mockStorage);

        // Assert
        subject.Should().BeAssignableTo<IGoalDataSource>();
        subject.Should().BeAssignableTo<DataSource<IGoalStorage, GoalEntity, uint>>();
    }
}
