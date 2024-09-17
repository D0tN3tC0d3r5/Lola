namespace Lola.Tasks.Repositories;

public class TaskStorageTests {
    [Fact]
    public void Constructor_ShouldInitializeCorrectly() {
        // Arrange
        var mockConfiguration = Substitute.For<IConfiguration>();

        // Act
        var subject = new TaskStorage(mockConfiguration);

        // Assert
        subject.Should().BeAssignableTo<ITaskStorage>();
        subject.Should().BeAssignableTo<JsonFilePerTypeStorage<TaskEntity, uint>>();
    }

    [Fact]
    public void Add_ShouldAssignIncrementalKeys() {
        // Arrange
        var mockConfiguration = Substitute.For<IConfiguration>();
        var subject = new TaskStorage(mockConfiguration);
        var entity1 = new TaskEntity { Name = "Alpha", Goals = ["Some goal."] };
        var entity2 = new TaskEntity { Name = "Bravo", Goals = ["Some goal."] };

        // Act
        subject.Add(entity1);
        subject.Add(entity2);

        // Assert
        entity1.Key.Should().Be(1u);
        entity2.Key.Should().Be(2u);
    }

    [Fact]
    public void GetAll_ShouldReturnEntitiesWithCorrectKeys() {
        // Arrange
        var mockConfiguration = Substitute.For<IConfiguration>();
        var subject = new TaskStorage(mockConfiguration);
        var entity1 = new TaskEntity { Name = "Alpha", Goals = ["Some goal."] };
        var entity2 = new TaskEntity { Name = "Bravo", Goals = ["Some goal."] };

        // Act
        subject.Add(entity1);
        subject.Add(entity2);
        var allEntities = subject.GetAll().ToList();

        // Assert
        allEntities.Should().HaveCount(2);
        allEntities[0].Key.Should().Be(1u);
        allEntities[1].Key.Should().Be(2u);
    }
}
