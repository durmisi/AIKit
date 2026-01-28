using AIKit.Clients.Claude;
using AIKit.Clients.Settings;
using Microsoft.Extensions.AI;
using Shouldly;
using Xunit;

namespace AIKit.Clients.Tests.Claude;

public class ChatClientFactoryTests
{
    [Fact]
    public void Provider_ReturnsCorrectName()
    {
        // Arrange
        var settings = new AIClientSettings { ApiKey = "test", ModelId = "claude-3-5-sonnet-20241022" };
        var factory = new ChatClientFactory(settings);

        // Act
        var result = factory.Provider;

        // Assert
        result.ShouldBe("claude");
    }

    [Fact]
    public void Provider_ReturnsCustomName_WhenSet()
    {
        // Arrange
        var settings = new AIClientSettings { ApiKey = "test", ModelId = "claude-3-5-sonnet-20241022", ProviderName = "custom-claude" };
        var factory = new ChatClientFactory(settings);

        // Act
        var result = factory.Provider;

        // Assert
        result.ShouldBe("custom-claude");
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Arrange
        var settings = new AIClientSettings { ApiKey = null, ModelId = "claude-3-5-sonnet-20241022" };

        // Act
        Action act = () => new ChatClientFactory(settings);

        // Assert
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldContain("ApiKey is required");
    }

    [Fact]
    public void Create_ReturnsChatClient()
    {
        // Arrange
        var settings = new AIClientSettings { ApiKey = "test-key", ModelId = "claude-3-5-sonnet-20241022" };
        var factory = new ChatClientFactory(settings);

        // Act
        var client = factory.Create();

        // Assert
        client.ShouldNotBeNull();
        client.ShouldBeAssignableTo<IChatClient>();
    }

    [Fact]
    public void Create_WithModel_ReturnsChatClient()
    {
        // Arrange
        var settings = new AIClientSettings { ApiKey = "test-key", ModelId = "claude-3-5-sonnet-20241022" };
        var factory = new ChatClientFactory(settings);

        // Act
        var client = factory.Create("claude-3-haiku-20240307");

        // Assert
        client.ShouldNotBeNull();
        client.ShouldBeAssignableTo<IChatClient>();
    }
}