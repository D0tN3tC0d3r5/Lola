namespace Lola.Goals.Repositories;

public class GoalStorageTests {
    [Fact]
    public void Constructor_ShouldInitializeCorrectly() {
        // Arrange
        var mockConfiguration = Substitute.For<IConfiguration>();

        // Act
        var subject = new GoalStorage(mockConfiguration);

        // Assert
        subject.Should().BeAssignableTo<IGoalStorage>();
        subject.Should().BeAssignableTo<JsonFilePerTypeStorage<GoalEntity, uint>>();
    }

    [Fact]
    public void Add_ShouldAssignIncrementalIds() {
        // Arrange
        var mockConfiguration = Substitute.For<IConfiguration>();
        var taskHandler = Substitute.For<IGoalHandler>();
        var context = new Map {
            [nameof(EntityAction)] = EntityAction.Insert,
            [nameof(GoalHandler)] = taskHandler,
        };
        var subject = new GoalStorage(mockConfiguration);
        var entity1 = new GoalEntity { Name = "Alpha", Objectives = ["Some objective."] };
        var entity2 = new GoalEntity { Name = "Bravo", Objectives = ["Some objective."] };

        // Act
        subject.Add(entity1, context);
        subject.Add(entity2, context);

        // Assert
        entity1.Id.Should().Be(1u);
        entity2.Id.Should().Be(2u);
    }

    [Fact]
    public void GetAll_ShouldReturnEntitiesWithCorrectIds() {
        // Arrange
        var mockConfiguration = Substitute.For<IConfiguration>();
        var taskHandler = Substitute.For<IGoalHandler>();
        var context = new Map {
            [nameof(EntityAction)] = EntityAction.Insert,
            [nameof(GoalHandler)] = taskHandler,
        };
        var subject = new GoalStorage(mockConfiguration);
        var entity1 = new GoalEntity { Name = "Alpha", Objectives = ["Some objective."] };
        var entity2 = new GoalEntity { Name = "Bravo", Objectives = ["Some objective."] };

        // Act
        subject.Add(entity1, context);
        subject.Add(entity2, context);
        var allEntities = subject.GetAll().ToList();

        // Assert
        allEntities.Should().HaveCount(2);
        allEntities[0].Id.Should().Be(1u);
        allEntities[1].Id.Should().Be(2u);
    }
}
