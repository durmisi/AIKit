using Microsoft.Extensions.AI;
using Shouldly;
using Xunit;

namespace AIKit.Clients.Tests.AzureClaude;

public class ChatClientBuilderTests
{
    [Fact]
    public void Provider_ReturnsCorrectName()
    {
        // Arrange
        var builder = new AIKit.Clients.AzureClaude.ChatClientBuilder()
            .WithApiKey("test")
            .WithEndpoint("https://test.claude.azure.com")
            .WithModel("claude-sonnet-4-5");

        // Act
        var result = builder.Provider;

        // Assert
        result.ShouldBe("azure-claude");
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Arrange
        var builder = new AIKit.Clients.AzureClaude.ChatClientBuilder()
            .WithEndpoint("https://test.claude.azure.com")
            .WithModel("claude-sonnet-4-5");

        // Act
        Action act = () => builder.Create();

        // Assert
        act.ShouldThrow<ArgumentException>().Message.ShouldContain("ApiKey is required");
    }

    [Fact]
    public void Create_Throws_WhenEndpointInvalid()
    {
        // Arrange
        var builder = new AIKit.Clients.AzureClaude.ChatClientBuilder()
            .WithApiKey("test")
            .WithEndpoint("invalid-url")
            .WithModel("claude-sonnet-4-5");

        // Act
        Action act = () => builder.Create();

        // Assert
        act.ShouldThrow<ArgumentException>().Message.ShouldContain("Endpoint must be a valid absolute URI");
    }

    [Fact]
    public void Create_Throws_WhenModelInvalid()
    {
        // Arrange
        var builder = new AIKit.Clients.AzureClaude.ChatClientBuilder()
            .WithApiKey("test")
            .WithEndpoint("https://test.claude.azure.com");

        // Act
        Action act = () => builder.Create();

        // Assert
        act.ShouldThrow<ArgumentException>().Message.ShouldContain("ModelId is required");
    }

    [Fact]
    public void Create_ReturnsChatClient()
    {
        // Arrange
        var builder = new AIKit.Clients.AzureClaude.ChatClientBuilder()
            .WithApiKey("test-key")
            .WithEndpoint("https://test.claude.azure.com")
            .WithModel("claude-sonnet-4-5");

        // Act
        var client = builder.Create();

        // Assert
        client.ShouldNotBeNull();
        client.ShouldBeAssignableTo<IChatClient>();
    }
}