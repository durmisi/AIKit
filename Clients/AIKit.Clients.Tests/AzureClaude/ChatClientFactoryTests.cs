using AIKit.Clients.AzureClaude;
using Microsoft.Extensions.AI;
using Shouldly;
using Xunit;

namespace AIKit.Clients.Tests.AzureClaude;

public class ChatClientFactoryTests
{
    [Fact]
    public void Provider_ReturnsCorrectName()
    {
        // Arrange
        var settings = new AIClientSettings { ApiKey = "test", Endpoint = "https://test.claude.azure.com", ModelId = "claude-sonnet-4-5" };
        var factory = new ChatClientFactory(settings);

        // Act
        var result = factory.Provider;

        // Assert
        result.ShouldBe("azure-claude");
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Arrange
        var settings = new AIClientSettings { ApiKey = null, Endpoint = "https://test.claude.azure.com", ModelId = "claude-sonnet-4-5" };

        // Act
        Action act = () => new ChatClientFactory(settings);

        // Assert
        act.ShouldThrow<ArgumentException>().Message.ShouldContain("ApiKey is required");
    }

    [Fact]
    public void Create_ReturnsChatClient()
    {
        // Arrange
        var settings = new AIClientSettings { ApiKey = "test-key", Endpoint = "https://test.claude.azure.com", ModelId = "claude-sonnet-4-5" };
        var factory = new ChatClientFactory(settings);

        // Act
        var client = factory.Create();

        // Assert
        client.ShouldNotBeNull();
        client.ShouldBeAssignableTo<IChatClient>();
    }
}









