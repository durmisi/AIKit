using AIKit.Clients.OpenAI;
using AIKit.Clients.Settings;
using Microsoft.Extensions.AI;
using Shouldly;
using Xunit;

namespace AIKit.Clients.Tests.OpenAI;

public class ChatClientBuilderTests
{
    [Fact]
    public void Provider_ReturnsCorrectName()
    {
        // Arrange
        var builder = new AIKit.Clients.OpenAI.ChatClientBuilder()
            .WithApiKey("test")
            .WithModel("gpt-4");

        // Act
        var result = builder.Provider;

        // Assert
        result.ShouldBe("open-ai");
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Arrange
        var builder = new AIKit.Clients.OpenAI.ChatClientBuilder()
            .WithModel("gpt-4");

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
        var builder = new AIKit.Clients.OpenAI.ChatClientBuilder()
            .WithApiKey("test-key")
            .WithModel("gpt-4");

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
        var builder = new AIKit.Clients.OpenAI.ChatClientBuilder()
            .WithApiKey("test-key")
            .WithModel("gpt-4");

        // Act
        var client = builder.Create("gpt-3.5-turbo");

        // Assert
        client.ShouldNotBeNull();
        client.ShouldBeAssignableTo<IChatClient>();
    }
}