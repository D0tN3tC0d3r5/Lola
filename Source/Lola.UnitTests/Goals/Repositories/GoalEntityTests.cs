using TaskResponseType = DotNetToolbox.AI.Jobs.TaskResponseType;

namespace Lola.Goals.Repositories;

public class GoalEntityTests {
    private readonly IMap _mockContext;

    public GoalEntityTests() {
        var mockGoalHandler = Substitute.For<IGoalHandler>();
        _mockContext = Substitute.For<IMap>();
        _mockContext.GetRequiredValueAs<IGoalHandler>(nameof(GoalHandler)).Returns(mockGoalHandler);
    }

    [Fact]
    public void Validate_WithValidEntity_ShouldReturnSuccess() {
        // Arrange
        var entity = new GoalEntity {
            Id = 1,
            Name = "Test Task",
            Objectives = ["Objective 1"],
        };

        // Act
        var result = entity.Validate(_mockContext);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldReturnError() {
        // Arrange
        var entity = new GoalEntity {
            Id = 1,
            Name = "",
            Objectives = ["Objective 1"],
        };

        // Act
        var result = entity.Validate(_mockContext);

        // Assert
        result.IsInvalid.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "The name is required.");
    }

    [Fact]
    public void Validate_WithEmptyObjectives_ShouldReturnError() {
        // Arrange
        var entity = new GoalEntity {
            Id = 1,
            Name = "Test Task",
            Objectives = [],
        };

        // Act
        var result = entity.Validate(_mockContext);

        // Assert
        result.IsInvalid.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "At least one objective is required.");
    }

    [Fact]
    public void Validate_WithEmptyNameAndObjectives_ShouldReturnMultipleErrors() {
        // Arrange
        var entity = new GoalEntity {
            Id = 1,
            Name = "",
            Objectives = [],
        };

        // Act
        var result = entity.Validate(_mockContext);

        // Assert
        result.IsInvalid.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "The name is required.");
        result.Errors.Should().Contain(e => e.Message == "At least one objective is required.");
    }

    [Fact]
    public void ImplicitConversion_ToTask_ShouldConvertCorrectly() {
        // Arrange
        var entity = new GoalEntity {
            Id = 1,
            Name = "Test Task",
            Objectives = ["Objective 1", "Objective 2"],
            Scope = ["Scope 1"],
            Requirements = ["Requirement 1"],
            Assumptions = ["Assumption 1"],
            Constraints = ["Constraint 1"],
            Examples = ["Example 1"],
            Guidelines = ["Guideline 1"],
            Validations = ["Validation 1"],
            InputTemplate = "Input Template",
            ResponseType = TaskResponseType.Json,
            ResponseSchema = "Response Schema",
        };

        // Act
        DotNetToolbox.AI.Jobs.Task task = entity;

        // Assert
        task.Id.Should().Be(entity.Id);
        task.Name.Should().Be(entity.Name);
        task.Goals.Should().BeEquivalentTo(entity.Objectives);
        task.Scope.Should().BeEquivalentTo(entity.Scope);
        task.Requirements.Should().BeEquivalentTo(entity.Requirements);
        task.Assumptions.Should().BeEquivalentTo(entity.Assumptions);
        task.Constraints.Should().BeEquivalentTo(entity.Constraints);
        task.Examples.Should().BeEquivalentTo(entity.Examples);
        task.Guidelines.Should().BeEquivalentTo(entity.Guidelines);
        task.Validations.Should().BeEquivalentTo(entity.Validations);
        task.InputTemplate.Should().Be(entity.InputTemplate);
        task.ResponseType.Should().Be(entity.ResponseType);
        task.ResponseSchema.Should().Be(entity.ResponseSchema);
    }
}
