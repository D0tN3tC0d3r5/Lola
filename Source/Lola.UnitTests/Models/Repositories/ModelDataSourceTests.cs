namespace Lola.Models.Repositories;

public class ModelDataSourceTests {
    [Fact]
    public void Constructor_ShouldInitializeCorrectly() {
        // Arrange
        var mockStorage = Substitute.For<IModelStorage>();

        // Act
        var subject = new ModelDataSource(mockStorage);

        // Assert
        subject.Should().BeAssignableTo<IModelDataSource>();
        subject.Should().BeAssignableTo<DataSource<IModelStorage, ModelEntity, uint>>();
    }

    [Fact]
    public void GetAll_WithoutIncludingProviders_ShouldReturnModelsWithoutProviders() {
        // Arrange
        var mockStorage = Substitute.For<IModelStorage>();
        var subject = new ModelDataSource(mockStorage);

        var models = new[]
        {
            new ModelEntity { Id = 1, Key = "model1", ProviderId = 1 },
            new ModelEntity { Id = 2, Key = "model2", ProviderId = 2 },
        };

        mockStorage.GetAll(Arg.Any<Expression<Func<ModelEntity, bool>>?>()).Returns(models);

        // Act
        var result = subject.GetAll();

        // Assert
        result.Should().BeEquivalentTo(models);
        result.All(m => m.Provider == null).Should().BeTrue();
    }

    [Fact]
    public void GetAll_WithIncludingProviders_ShouldReturnModelsWithProviders() {
        // Arrange
        var mockStorage = Substitute.For<IModelStorage>();
        var mockProviderDataSource = Substitute.For<IProviderDataSource>();
        var subject = new ModelDataSource(mockStorage);

        var models = new[]
        {
            new ModelEntity { Id = 1, Key = "model1", ProviderId = 1 },
            new ModelEntity { Id = 2, Key = "model2", ProviderId = 2 },
        };

        var providers = new[]
        {
            new ProviderEntity { Id = 1, Name = "Provider1" },
            new ProviderEntity { Id = 2, Name = "Provider2" },
        };

        mockStorage.GetAll(Arg.Any<Expression<Func<ModelEntity, bool>>?>()).Returns(models);
        mockProviderDataSource.GetAll().Returns(providers);

        // Act
        var result = subject.GetAll();

        // Assert
        result.Should().HaveCount(2);
        result[0].Provider.Should().BeEquivalentTo(providers[0]);
        result[1].Provider.Should().BeEquivalentTo(providers[1]);
    }

    [Fact]
    public void GetSelected_ShouldReturnSelectedModelWithProvider() {
        // Arrange
        var mockStorage = Substitute.For<IModelStorage>();
        var mockProviderDataSource = Substitute.For<IProviderDataSource>();
        var subject = new ModelDataSource(mockStorage);

        var selectedModel = new ModelEntity { Id = 1, Key = "selected", ProviderId = 1, Selected = true };
        var provider = new ProviderEntity { Id = 1, Name = "Provider1" };

        mockStorage.Find(Arg.Any<Expression<Func<ModelEntity, bool>>>()).Returns(selectedModel);
        mockProviderDataSource.FindByKey(1u).Returns(provider);

        // Act
        var result = subject.Find(p => p.Selected);

        // Assert
        result.Should().BeEquivalentTo(selectedModel);
        result.Provider.Should().BeEquivalentTo(provider);
    }

    [Fact]
    public void GetSelected_WithInvalidKey_ReturnsNull() {
        // Arrange
        var mockStorage = Substitute.For<IModelStorage>();
        var subject = new ModelDataSource(mockStorage);

        mockStorage.Find(Arg.Any<Expression<Func<ModelEntity, bool>>>()).Returns((ModelEntity?)null);

        // Act
        var result = subject.Find(p => p.Selected);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FindByKey_WithIncludeProvider_ShouldReturnModelWithProvider() {
        // Arrange
        var mockStorage = Substitute.For<IModelStorage>();
        var mockProviderDataSource = Substitute.For<IProviderDataSource>();
        var subject = new ModelDataSource(mockStorage);

        var model = new ModelEntity { Id = 1, Key = "model1", ProviderId = 1 };
        var provider = new ProviderEntity { Id = 1, Name = "Provider1" };

        mockStorage.FindByKey(1).Returns(model);
        mockProviderDataSource.FindByKey(1u).Returns(provider);

        // Act
        var result = subject.Find(p => p.Id == 1);

        // Assert
        result.Should().BeEquivalentTo(model);
        result.Provider.Should().BeEquivalentTo(provider);
    }

    [Fact]
    public void FindByKey_WithoutIncludeProvider_ShouldReturnModelWithoutProvider() {
        // Arrange
        var mockStorage = Substitute.For<IModelStorage>();
        var subject = new ModelDataSource(mockStorage);

        var model = new ModelEntity { Id = 1, Key = "model1", ProviderId = 1 };

        mockStorage.FindByKey(1).Returns(model);

        // Act
        var result = subject.Find(p => p.Id == 1);

        // Assert
        result.Should().BeEquivalentTo(model);
        result.Provider.Should().BeNull();
    }

    [Fact]
    public void FindByKey_WithInvalidKey_ReturnsNull() {
        // Arrange
        var mockStorage = Substitute.For<IModelStorage>();
        var mockProviderDataSource = Substitute.For<IProviderDataSource>();
        var subject = new ModelDataSource(mockStorage);

        mockStorage.Find(Arg.Any<Expression<Func<ModelEntity, bool>>>()).Returns((ModelEntity?)null);

        // Act
        var result = subject.FindByKey(13);

        // Assert
        result.Should().BeNull();
    }
}
