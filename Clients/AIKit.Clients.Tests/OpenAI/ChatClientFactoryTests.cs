using AIKit.Clients.OpenAI;
using AIKit.Clients.Settings;
using Microsoft.Extensions.AI;
using Shouldly;
using Xunit;

namespace AIKit.Clients.Tests.OpenAI;

public class ChatClientFactoryTests
{
    [Fact]
    public void Provider_ReturnsCorrectName()
    {
        // Arrange
        var settings = new AIClientSettings { ApiKey = "test", ModelId = "gpt-4" };
        var factory = new ChatClientFactory(settings);

        // Act
        var result = factory.Provider;

        // Assert
        result.ShouldBe("open-ai");
    }

    [Fact]
    public void Provider_ReturnsCustomName_WhenSet()
    {
        // Arrange
        var settings = new AIClientSettings { ApiKey = "test", ModelId = "gpt-4", ProviderName = "custom-openai" };
        var factory = new ChatClientFactory(settings);

        // Act
        var result = factory.Provider;

        // Assert
        result.ShouldBe("custom-openai");
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Arrange
        var settings = new AIClientSettings { ApiKey = null, ModelId = "gpt-4" };

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
        var settings = new AIClientSettings { ApiKey = "test-key", ModelId = "gpt-4" };
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
        var settings = new AIClientSettings { ApiKey = "test-key", ModelId = "gpt-4" };
        var factory = new ChatClientFactory(settings);

        // Act
        var client = factory.Create("gpt-3.5-turbo");

        // Assert
        client.ShouldNotBeNull();
        client.ShouldBeAssignableTo<IChatClient>();
    }
}