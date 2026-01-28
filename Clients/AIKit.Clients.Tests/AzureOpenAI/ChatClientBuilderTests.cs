using Microsoft.Extensions.AI;
using Shouldly;
using Xunit;

namespace AIKit.Clients.Tests.AzureOpenAI;

public class ChatClientBuilderTests
{
    [Fact]
    public void Provider_ReturnsCorrectName()
    {
        // Arrange
        var builder = new AIKit.Clients.AzureOpenAI.ChatClientBuilder()
            .WithApiKey("test")
            .WithEndpoint("https://test.openai.azure.com")
            .WithModel("gpt-4");

        // Act
        var result = builder.Provider;

        // Assert
        result.ShouldBe("azure-open-ai");
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Arrange
        var builder = new AIKit.Clients.AzureOpenAI.ChatClientBuilder()
            .WithEndpoint("https://test.openai.azure.com")
            .WithModel("gpt-4");

        // Act
        Action act = () => builder.Build();

        // Assert
        act.ShouldThrow<ArgumentException>().Message.ShouldContain("ApiKey is required");
    }

    [Fact]
    public void Create_ReturnsChatClient()
    {
        // Arrange
        var builder = new AIKit.Clients.AzureOpenAI.ChatClientBuilder()
            .WithApiKey("test-key")
            .WithEndpoint("https://test.openai.azure.com")
            .WithModel("gpt-4");

        // Act
        var client = builder.Build();

        // Assert
        client.ShouldNotBeNull();
        client.ShouldBeAssignableTo<IChatClient>();
    }
}