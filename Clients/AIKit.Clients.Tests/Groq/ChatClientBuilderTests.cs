using AIKit.Clients.Groq;
using AIKit.Clients.Settings;
using Microsoft.Extensions.AI;
using Shouldly;
using Xunit;

namespace AIKit.Clients.Tests.Groq;

public class ChatClientBuilderTests
{
    [Fact]
    public void Provider_ReturnsCorrectName()
    {
        // Arrange
        var builder = new AIKit.Clients.Groq.ChatClientBuilder()
            .WithApiKey("test")
            .WithModel("llama-3.3-70b-versatile");

        // Act
        var result = builder.Provider;

        // Assert
        result.ShouldBe("groq");
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Arrange
        var builder = new AIKit.Clients.Groq.ChatClientBuilder()
            .WithModel("llama-3.3-70b-versatile");

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
        var builder = new AIKit.Clients.Groq.ChatClientBuilder()
            .WithApiKey("test-key")
            .WithModel("llama-3.3-70b-versatile");

        // Act
        var client = builder.Create();

        // Assert
        client.ShouldNotBeNull();
        client.ShouldBeAssignableTo<IChatClient>();
    }
}