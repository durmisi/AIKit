using AIKit.Clients.Claude;
using AIKit.Clients.Interfaces;
using AIKit.Clients.Settings;
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
        Action act = () => builder.Create();

        // Assert
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldContain("ApiKey is required");
    }

    [Fact]
    public void Create_ReturnsChatClient()
    {
        // Arrange
        var builder = new AIKit.Clients.Claude.ChatClientBuilder()
            .WithApiKey("test-key")
            .WithModel("claude-3-5-sonnet-20241022");

        // Act
        var client = builder.Create();

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
        var client = builder.Create("claude-3-haiku-20240307");

        // Assert
        client.ShouldNotBeNull();
        client.ShouldBeAssignableTo<IChatClient>();
    }
}