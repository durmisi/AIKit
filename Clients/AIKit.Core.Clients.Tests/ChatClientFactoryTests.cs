using AIKit.Core.Clients;
using FluentAssertions;
using Microsoft.Extensions.AI;
using Moq;
using Xunit;

namespace AIKit.Core.Clients.Tests;

public class ChatClientFactoryTests
{
    [Fact]
    public void Constructor_InitializesWithProviders()
    {
        // Arrange
        var mockProvider = new Mock<IChatClientProvider>();
        mockProvider.Setup(p => p.Provider).Returns("test-provider");
        var providers = new[] { mockProvider.Object };

        // Act
        var factory = new ChatClientFactory(providers);

        // Assert
        factory.Should().NotBeNull();
    }

    [Fact]
    public void AddProvider_AddsProviderSuccessfully()
    {
        // Arrange
        var factory = new ChatClientFactory(Enumerable.Empty<IChatClientProvider>());
        var mockProvider = new Mock<IChatClientProvider>();
        mockProvider.Setup(p => p.Provider).Returns("added-provider");

        // Act
        factory.AddProvider(mockProvider.Object);

        // Assert
        // Test by trying to create, which should succeed
        var mockClient = new Mock<IChatClient>();
        mockProvider.Setup(p => p.Create(It.IsAny<string>())).Returns(mockClient.Object);

        var result = factory.Create("added-provider", "model");
        result.Should().Be(mockClient.Object);
    }

    [Fact]
    public void Create_WithValidProvider_ReturnsClient()
    {
        // Arrange
        var mockProvider = new Mock<IChatClientProvider>();
        mockProvider.Setup(p => p.Provider).Returns("test-provider");
        var mockClient = new Mock<IChatClient>();
        mockProvider.Setup(p => p.Create(It.IsAny<string>())).Returns(mockClient.Object);
        var factory = new ChatClientFactory(new[] { mockProvider.Object });

        // Act
        var result = factory.Create("test-provider", "model");

        // Assert
        result.Should().Be(mockClient.Object);
    }

    [Fact]
    public void Create_WithInvalidProvider_ThrowsException()
    {
        // Arrange
        var factory = new ChatClientFactory(Enumerable.Empty<IChatClientProvider>());

        // Act
        Action act = () => factory.Create("non-existent", "model");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*No IChatClientProvider registered*");
    }

    [Fact]
    public void Create_WithNullProvider_ThrowsException()
    {
        // Arrange
        var factory = new ChatClientFactory(Enumerable.Empty<IChatClientProvider>());

        // Act
        Action act = () => factory.Create(null!, "model");

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("provider");
    }

    [Fact]
    public void Create_WithNullModel_ThrowsException()
    {
        // Arrange
        var factory = new ChatClientFactory(Enumerable.Empty<IChatClientProvider>());

        // Act
        Action act = () => factory.Create("provider", null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("model");
    }
}