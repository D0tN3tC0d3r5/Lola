namespace Lola.Personas.Repositories;

public class PersonaStorageTests {
    [Fact]
    public void Constructor_ShouldInitializeCorrectly() {
        // Arrange
        var mockConfiguration = Substitute.For<IConfiguration>();

        // Act
        var subject = new PersonaStorage(mockConfiguration);

        // Assert
        subject.Should().BeAssignableTo<IPersonaStorage>();
        subject.Should().BeAssignableTo<JsonFilePerTypeStorage<PersonaEntity, uint>>();
    }

    [Fact]
    public void Add_ShouldAssignIncrementalKeys() {
        // Arrange
        var mockConfiguration = Substitute.For<IConfiguration>();
        var personaHandler = Substitute.For<IPersonaHandler>();
        var context = new Map { ["PersonaHandler"] = personaHandler };
        var subject = new PersonaStorage(mockConfiguration);
        var entity1 = new PersonaEntity { Name = "Alpha", Role = "Boss", Goals = ["Some goal."] };
        var entity2 = new PersonaEntity { Name = "Bravo", Role = "Assistant", Goals = ["Some goal."] };

        // Act
        subject.Add(entity1, context);
        subject.Add(entity2, context);

        // Assert
        entity1.Key.Should().Be(1u);
        entity2.Key.Should().Be(2u);
    }

    [Fact]
    public void GetAll_ShouldReturnEntitiesWithCorrectKeys() {
        // Arrange
        var mockConfiguration = Substitute.For<IConfiguration>();
        var personaHandler = Substitute.For<IPersonaHandler>();
        var context = new Map { ["PersonaHandler"] = personaHandler };
        var subject = new PersonaStorage(mockConfiguration);
        var entity1 = new PersonaEntity { Name = "Alpha", Role = "Boss", Goals = ["Some goal."] };
        var entity2 = new PersonaEntity { Name = "Bravo", Role = "Assistant", Goals = ["Some goal."] };

        // Act
        subject.Add(entity1, context);
        subject.Add(entity2, context);
        var allEntities = subject.GetAll().ToList();

        // Assert
        allEntities.Should().HaveCount(2);
        allEntities[0].Key.Should().Be(1u);
        allEntities[1].Key.Should().Be(2u);
    }
}
