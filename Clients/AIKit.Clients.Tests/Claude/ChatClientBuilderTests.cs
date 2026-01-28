using Microsoft.Extensions.AI;
using Shouldly;
using Xunit;

namespace AIKit.Clients.Tests.Claude;

public class ChatClientBuilderTests
{
    [Fact]
    public void Provider_ReturnsCorrectName()
    {
        // Arrange
        var builder = new AIKit.Clients.Claude.ChatClientBuilder()
            .WithApiKey("test")
            .WithModel("claude-3-5-sonnet-20241022");

        // Act
        var result = builder.Provider;

        // Assert
        result.ShouldBe("claude");
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Arrange
        var builder = new AIKit.Clients.Claude.ChatClientBuilder()
            .WithModel("claude-3-5-sonnet-20241022");

        // Act
        Action act = () => builder.Build();

        // Assert
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldContain("ApiKey is required");
    }

    [Fact]
    public void Create_Throws_WhenModelMissing()
    {
        // Arrange
        var builder = new AIKit.Clients.Claude.ChatClientBuilder()
            .WithApiKey("test-key");

        // Act
        Action act = () => builder.Build();

        // Assert
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldContain("ModelId is required");
    }

    [Fact]
    public void Create_ReturnsChatClient()
    {
        // Arrange
        var builder = new AIKit.Clients.Claude.ChatClientBuilder()
            .WithApiKey("test-key")
            .WithModel("claude-3-5-sonnet-20241022");

        // Act
        var client = builder.Build();

        // Assert
        client.ShouldNotBeNull();
        client.ShouldBeAssignableTo<IChatClient>();
    }

    [Fact]
    public void Create_WithModel_ReturnsChatClient()
    {
        // Arrange
        var builder = new AIKit.Clients.Claude.ChatClientBuilder()
            .WithApiKey("test-key")
            .WithModel("claude-3-5-sonnet-20241022");

        // Act
        var client = builder.WithModel("claude-3-haiku-20240307").Build();

        // Assert
        client.ShouldNotBeNull();
        client.ShouldBeAssignableTo<IChatClient>();
    }
}