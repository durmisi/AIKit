using AIKit.Core.Clients;
using FluentAssertions;
using Microsoft.Extensions.AI;
using Moq;
using Xunit;

namespace AIKit.Core.Clients.Tests;

public class EmbeddingGeneratorFactoryTests
{
    [Fact]
    public void Constructor_InitializesWithProviders()
    {
        // Arrange
        var mockProvider = new Mock<IEmbeddingProvider>();
        mockProvider.Setup(p => p.Provider).Returns("test-provider");
        var providers = new[] { mockProvider.Object };

        // Act
        var factory = new EmbeddingGeneratorFactory(providers);

        // Assert
        factory.Should().NotBeNull();
    }

    [Fact]
    public void AddProvider_AddsProviderSuccessfully()
    {
        // Arrange
        var factory = new EmbeddingGeneratorFactory(Enumerable.Empty<IEmbeddingProvider>());
        var mockProvider = new Mock<IEmbeddingProvider>();
        mockProvider.Setup(p => p.Provider).Returns("added-provider");

        // Act
        factory.AddProvider(mockProvider.Object);

        // Assert
        // Test by trying to create, which should succeed
        var mockGenerator = new Mock<IEmbeddingGenerator<string, Embedding<float>>>();
        mockProvider.Setup(p => p.Create()).Returns(mockGenerator.Object);

        var result = factory.Create("added-provider");
        result.Should().Be(mockGenerator.Object);
    }

    [Fact]
    public void Create_WithValidProvider_ReturnsGenerator()
    {
        // Arrange
        var mockProvider = new Mock<IEmbeddingProvider>();
        mockProvider.Setup(p => p.Provider).Returns("test-provider");
        var mockGenerator = new Mock<IEmbeddingGenerator<string, Embedding<float>>>();
        mockProvider.Setup(p => p.Create()).Returns(mockGenerator.Object);
        var factory = new EmbeddingGeneratorFactory(new[] { mockProvider.Object });

        // Act
        var result = factory.Create("test-provider");

        // Assert
        result.Should().Be(mockGenerator.Object);
    }

    [Fact]
    public void Create_WithCustomSettings_UsesSettings()
    {
        // Arrange
        var mockProvider = new Mock<IEmbeddingProvider>();
        mockProvider.Setup(p => p.Provider).Returns("test-provider");
        var mockGenerator = new Mock<IEmbeddingGenerator<string, Embedding<float>>>();
        var settings = new AIClientSettings { ModelId = "custom-model" };
        mockProvider.Setup(p => p.Create(settings)).Returns(mockGenerator.Object);
        var factory = new EmbeddingGeneratorFactory(new[] { mockProvider.Object });

        // Act
        var result = factory.Create("test-provider", settings);

        // Assert
        result.Should().Be(mockGenerator.Object);
        mockProvider.Verify(p => p.Create(settings), Times.Once);
    }

    [Fact]
    public void Create_WithInvalidProvider_ThrowsException()
    {
        // Arrange
        var factory = new EmbeddingGeneratorFactory(Enumerable.Empty<IEmbeddingProvider>());

        // Act
        Action act = () => factory.Create("non-existent");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*No IEmbeddingProvider registered*");
    }

    [Fact]
    public void Create_WithNullProvider_ThrowsException()
    {
        // Arrange
        var factory = new EmbeddingGeneratorFactory(Enumerable.Empty<IEmbeddingProvider>());

        // Act
        Action act = () => factory.Create(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("provider");
    }
}