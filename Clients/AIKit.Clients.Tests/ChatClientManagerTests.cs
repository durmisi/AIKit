using AIKit.Clients.Interfaces;
using AIKit.Clients.Managers;
using Microsoft.Extensions.AI;
using Moq;
using Shouldly;
using Xunit;

namespace AIKit.Clients.Tests;

public class ChatClientManagerTests
{
    [Fact]
    public void Constructor_InitializesWithProviders()
    {
        // Arrange
        var mockProvider = new Mock<IChatClientFactory>();
        mockProvider.Setup(p => p.Provider).Returns("test-provider");
        var providers = new[] { mockProvider.Object };

        // Act
        var manager = new ChatClientManager(providers);

        // Assert
        manager.ShouldNotBeNull();
    }

    [Fact]
    public void AddProvider_AddsProviderSuccessfully()
    {
        // Arrange
        var manager = new ChatClientManager([]);
        var mockProvider = new Mock<IChatClientFactory>();
        mockProvider.Setup(p => p.Provider).Returns("added-provider");

        // Act
        manager.AddProvider(mockProvider.Object);

        // Assert
        // Test by trying to create, which should succeed
        var mockClient = new Mock<IChatClient>();
        mockProvider.Setup(p => p.Create(It.IsAny<string>())).Returns(mockClient.Object);

        var result = manager.Create("added-provider", "model");
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
        var manager = new ChatClientManager([mockProvider.Object]);

        // Act
        var result = manager.Create("test-provider", "model");

        // Assert
        result.ShouldBe(mockClient.Object);
    }

    [Fact]
    public void Create_WithInvalidProvider_ThrowsException()
    {
        // Arrange
        var manager = new ChatClientManager([]);

        // Act
        Action act = () => manager.Create("non-existent", "model");

        // Assert
        act.ShouldThrow<InvalidOperationException>().Message.ShouldContain("No IChatClientFactory registered");
    }

    [Fact]
    public void Create_WithNullProvider_ThrowsException()
    {
        // Arrange
        var manager = new ChatClientManager([]);

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => manager.Create(null!, "model"));
    }

    [Fact]
    public void Create_WithNullModel_ThrowsException()
    {
        // Arrange
        var manager = new ChatClientManager(Enumerable.Empty<IChatClientFactory>());

        // Act
        Action act = () => manager.Create("provider", null!);

        // Assert
        act.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("model");
    }

    [Fact]
    public async Task CreateAndUseClient_IntegrationTest()
    {
        // Arrange
        var mockProvider = new Mock<IChatClientFactory>();
        mockProvider.Setup(p => p.Provider).Returns("test-provider");

        var mockClient = new Mock<IChatClient>();
        mockClient.Setup(c => c.GetResponseAsync(
            It.IsAny<IList<ChatMessage>>(),
            It.IsAny<ChatOptions>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatResponse(new ChatMessage(ChatRole.Assistant, "Test response")));

        mockProvider.Setup(p => p.Create(It.IsAny<string>())).Returns(mockClient.Object);

        var manager = new ChatClientManager([mockProvider.Object]);

        // Act
        var client = manager.Create("test-provider", "model");
        var response = await client.GetResponseAsync(
            new[] { new ChatMessage(ChatRole.User, "Hello") },
            new ChatOptions(),
            CancellationToken.None);

        // Assert
        client.ShouldNotBeNull();
        response.ShouldNotBeNull();
    }
}