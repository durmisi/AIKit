using AIKit.Clients.Groq;
using AIKit.Clients.Settings;
using Microsoft.Extensions.AI;
using Shouldly;
using Xunit;

namespace AIKit.Clients.Tests.Groq;

public class ChatClientFactoryTests
{
    [Fact]
    public void Provider_ReturnsCorrectName()
    {
        // Arrange
        var settings = new AIClientSettings { ApiKey = "test", ModelId = "llama-3.3-70b-versatile" };
        var factory = new ChatClientFactory(settings);

        // Act
        var result = factory.Provider;

        // Assert
        result.ShouldBe("groq");
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Arrange
        var settings = new AIClientSettings { ApiKey = null, ModelId = "llama-3.3-70b-versatile" };

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
        var settings = new AIClientSettings { ApiKey = "test-key", ModelId = "llama-3.3-70b-versatile" };
        var factory = new ChatClientFactory(settings);

        // Act
        var client = factory.Create();

        // Assert
        client.ShouldNotBeNull();
        client.ShouldBeAssignableTo<IChatClient>();
    }
}