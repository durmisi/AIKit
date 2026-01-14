using AIKit.Core.Clients;
using Shouldly;
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
        var mockProvider = new Mock<IChatClientFactory>();
        mockProvider.Setup(p => p.Provider).Returns("test-provider");
        var providers = new[] { mockProvider.Object };

        // Act
        var factory = new ChatClientFactoryFactory(providers);

        // Assert
        factory.ShouldNotBeNull();
    }

    [Fact]
    public void AddProvider_AddsProviderSuccessfully()
    {
        // Arrange
        var factory = new ChatClientFactoryFactory(Enumerable.Empty<IChatClientFactory>());
        var mockProvider = new Mock<IChatClientFactory>();
        mockProvider.Setup(p => p.Provider).Returns("added-provider");

        // Act
        factory.AddProvider(mockProvider.Object);

        // Assert
        // Test by trying to create, which should succeed
        var mockClient = new Mock<IChatClient>();
        mockProvider.Setup(p => p.Create(It.IsAny<string>())).Returns(mockClient.Object);

        var result = factory.Create("added-provider", "model");
        result.ShouldBe(mockClient.Object);
    }

    [Fact]
    public void Create_WithValidProvider_ReturnsClient()
    {
        // Arrange
        var mockProvider = new Mock<IChatClientFactory>();
        mockProvider.Setup(p => p.Provider).Returns("test-provider");
        var mockClient = new Mock<IChatClient>();
        mockProvider.Setup(p => p.Create(It.IsAny<string>())).Returns(mockClient.Object);
        var factory = new ChatClientFactoryFactory(new[] { mockProvider.Object });

        // Act
        var result = factory.Create("test-provider", "model");

        // Assert
        result.ShouldBe(mockClient.Object);
    }

    [Fact]
    public void Create_WithInvalidProvider_ThrowsException()
    {
        // Arrange
        var factory = new ChatClientFactoryFactory(Enumerable.Empty<IChatClientFactory>());

        // Act
        Action act = () => factory.Create("non-existent", "model");

        // Assert
        act.ShouldThrow<InvalidOperationException>().Message.ShouldContain("No IChatClientFactory registered");
    }

    [Fact]
    public void Create_WithNullProvider_ThrowsException()
    {
        // Arrange
        var factory = new ChatClientFactoryFactory(Enumerable.Empty<IChatClientFactory>());

        // Act
        Action act = () => factory.Create(null!, "model");

        // Assert
        act.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("provider");
    }

    [Fact]
    public void Create_WithNullModel_ThrowsException()
    {
        // Arrange
        var factory = new ChatClientFactoryFactory(Enumerable.Empty<IChatClientFactory>());

        // Act
        Action act = () => factory.Create("provider", null!);

        // Assert
        act.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("model");
    }
}









