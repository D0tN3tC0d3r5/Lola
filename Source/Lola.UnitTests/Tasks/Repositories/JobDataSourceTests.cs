using Lola.Jobs.Repositories;

namespace Lola.Tasks.Repositories;

public class JobDataSourceTests {
    [Fact]
    public void Constructor_ShouldInitializeCorrectly() {
        // Arrange
        var mockStorage = Substitute.For<IJobStorage>();

        // Act
        var subject = new JobDataSource(mockStorage);

        // Assert
        subject.Should().BeAssignableTo<IJobDataSource>();
        subject.Should().BeAssignableTo<DataSource<IJobStorage, JobEntity, uint>>();
    }
}
