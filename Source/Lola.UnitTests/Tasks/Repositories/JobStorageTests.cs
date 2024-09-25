using Lola.Jobs.Handlers;
using Lola.Jobs.Repositories;

namespace Lola.Tasks.Repositories;

public class JobStorageTests {
    [Fact]
    public void Constructor_ShouldInitializeCorrectly() {
        // Arrange
        var mockConfiguration = Substitute.For<IConfiguration>();

        // Act
        var subject = new JobStorage(mockConfiguration);

        // Assert
        subject.Should().BeAssignableTo<IJobStorage>();
        subject.Should().BeAssignableTo<JsonFilePerTypeStorage<JobEntity, uint>>();
    }

    [Fact]
    public void Add_ShouldAssignIncrementalIds() {
        // Arrange
        var mockConfiguration = Substitute.For<IConfiguration>();
        var taskHandler = Substitute.For<IJobHandler>();
        var context = new Map {
            [nameof(EntityAction)] = EntityAction.Insert,
            [nameof(JobHandler)] = taskHandler,
        };
        var subject = new JobStorage(mockConfiguration);
        var entity1 = new JobEntity { Name = "Alpha", Goals = ["Some goal."] };
        var entity2 = new JobEntity { Name = "Bravo", Goals = ["Some goal."] };

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
        var taskHandler = Substitute.For<IJobHandler>();
        var context = new Map {
            [nameof(EntityAction)] = EntityAction.Insert,
            [nameof(JobHandler)] = taskHandler,
        };
        var subject = new JobStorage(mockConfiguration);
        var entity1 = new JobEntity { Name = "Alpha", Goals = ["Some goal."] };
        var entity2 = new JobEntity { Name = "Bravo", Goals = ["Some goal."] };

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
