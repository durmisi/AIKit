using AIKit.Clients.Groq;
using AIKit.Core.Clients;
using Shouldly;
using Microsoft.Extensions.AI;
using Xunit;

namespace AIKit.Core.Clients.Tests.Groq;

public class ChatClientProviderTests
{
    [Fact]
    public void Provider_ReturnsCorrectName()
    {
        // Arrange
        var settings = new AIClientSettings { ApiKey = "test", ModelId = "llama-3.3-70b-versatile" };
        var provider = new ChatClientProvider(settings);

        // Act
        var result = provider.Provider;

        // Assert
        result.ShouldBe("groq");
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Arrange
        var settings = new AIClientSettings { ApiKey = null, ModelId = "llama-3.3-70b-versatile" };

        // Act
        Action act = () => new ChatClientProvider(settings);

        // Assert
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldContain("ApiKey is required");
    }

    [Fact]
    public void Create_ReturnsChatClient()
    {
        // Arrange
        var settings = new AIClientSettings { ApiKey = "test-key", ModelId = "llama-3.3-70b-versatile" };
        var provider = new ChatClientProvider(settings);

        // Act
        var client = provider.Create();

        // Assert
        client.ShouldNotBeNull();
        client.ShouldBeAssignableTo<IChatClient>();
    }
}









