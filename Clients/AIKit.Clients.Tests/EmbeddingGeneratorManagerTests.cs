using AIKit.Clients.Interfaces;
using AIKit.Clients.Managers;
using AIKit.Clients.Settings;
using Microsoft.Extensions.AI;
using Moq;
using Shouldly;
using Xunit;

namespace AIKit.Clients.Tests;

public class EmbeddingGeneratorManagerTests
{
    [Fact]
    public void Constructor_InitializesWithProviders()
    {
        // Arrange
        var mockProvider = new Mock<IEmbeddingGeneratorFactory>();
        mockProvider.Setup(p => p.Provider).Returns("test-provider");
        var providers = new[] { mockProvider.Object };

        // Act
        var manager = new EmbeddingGeneratorManager(providers);

        // Assert
        manager.ShouldNotBeNull();
    }

    [Fact]
    public void AddProvider_AddsProviderSuccessfully()
    {
        // Arrange
        var manager = new EmbeddingGeneratorManager(Enumerable.Empty<IEmbeddingGeneratorFactory>());
        var mockProvider = new Mock<IEmbeddingGeneratorFactory>();
        mockProvider.Setup(p => p.Provider).Returns("added-provider");

        // Act
        manager.AddProvider(mockProvider.Object);

        // Assert
        // Test by trying to create, which should succeed
        var mockGenerator = new Mock<IEmbeddingGenerator<string, Embedding<float>>>();
        mockProvider.Setup(p => p.Create()).Returns(mockGenerator.Object);

        var result = manager.Create("added-provider");
        result.ShouldBe(mockGenerator.Object);
    }

    [Fact]
    public void Create_WithValidProvider_ReturnsGenerator()
    {
        // Arrange
        var mockProvider = new Mock<IEmbeddingGeneratorFactory>();
        mockProvider.Setup(p => p.Provider).Returns("test-provider");
        var mockGenerator = new Mock<IEmbeddingGenerator<string, Embedding<float>>>();
        mockProvider.Setup(p => p.Create()).Returns(mockGenerator.Object);
        var manager = new EmbeddingGeneratorManager(new[] { mockProvider.Object });

        // Act
        var result = manager.Create("test-provider");

        // Assert
        result.ShouldBe(mockGenerator.Object);
    }

    [Fact]
    public void Create_WithCustomSettings_UsesSettings()
    {
        // Arrange
        var mockProvider = new Mock<IEmbeddingGeneratorFactory>();
        mockProvider.Setup(p => p.Provider).Returns("test-provider");
        var mockGenerator = new Mock<IEmbeddingGenerator<string, Embedding<float>>>();
        var settings = new AIClientSettings { ModelId = "custom-model" };
        mockProvider.Setup(p => p.Create(settings)).Returns(mockGenerator.Object);
        var manager = new EmbeddingGeneratorManager(new[] { mockProvider.Object });

        // Act
        var result = manager.Create("test-provider", settings);

        // Assert
        result.ShouldBe(mockGenerator.Object);
        mockProvider.Verify(p => p.Create(settings), Times.Once);
    }

    [Fact]
    public void Create_WithInvalidProvider_ThrowsException()
    {
        // Arrange
        var manager = new EmbeddingGeneratorManager(Enumerable.Empty<IEmbeddingGeneratorFactory>());

        // Act
        Action act = () => manager.Create("non-existent");

        // Assert
        act.ShouldThrow<InvalidOperationException>()
            .Message.ShouldContain("No IEmbeddingGeneratorFactory registered");
    }

    [Fact]
    public void Create_WithNullProvider_ThrowsException()
    {
        // Arrange
        var manager = new EmbeddingGeneratorManager(Enumerable.Empty<IEmbeddingGeneratorFactory>());

        // Act
        Action act = () => manager.Create(null!);

        // Assert
        act.ShouldThrow<ArgumentNullException>()
            .ParamName.ShouldBe("provider");
    }

    [Fact]
    public async Task CreateAndUseGenerator_IntegrationTest()
    {
        // Arrange
        var mockProvider = new Mock<IEmbeddingGeneratorFactory>();
        mockProvider.Setup(p => p.Provider).Returns("test-provider");

        var mockGenerator = new Mock<IEmbeddingGenerator<string, Embedding<float>>>();
        mockGenerator.Setup(g => g.GenerateAsync(
            It.IsAny<IEnumerable<string>>(),
            It.IsAny<EmbeddingGenerationOptions>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GeneratedEmbeddings<Embedding<float>>(new[] { new Embedding<float>(new float[] { 0.1f, 0.2f }) }));

        mockProvider.Setup(p => p.Create()).Returns(mockGenerator.Object);

        var manager = new EmbeddingGeneratorManager(new[] { mockProvider.Object });

        // Act
        var generator = manager.Create("test-provider");
        var embeddings = await generator.GenerateAsync(new[] { "test text" });

        // Assert
        generator.ShouldNotBeNull();
        embeddings.ShouldNotBeNull();
    }
}