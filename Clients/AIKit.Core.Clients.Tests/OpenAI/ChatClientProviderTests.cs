using AIKit.Clients.OpenAI;
using AIKit.Core.Clients;
using FluentAssertions;
using Microsoft.Extensions.AI;
using Xunit;

namespace AIKit.Core.Clients.Tests.OpenAI;

public class ChatClientProviderTests
{
    [Fact]
    public void Provider_ReturnsCorrectName()
    {
        // Arrange
        var settings = new AIClientSettings { ApiKey = "test", ModelId = "gpt-4" };
        var provider = new ChatClientProvider(settings);

        // Act
        var result = provider.Provider;

        // Assert
        result.Should().Be("open-ai");
    }

    [Fact]
    public void Provider_ReturnsCustomName_WhenSet()
    {
        // Arrange
        var settings = new AIClientSettings { ApiKey = "test", ModelId = "gpt-4", ProviderName = "custom-openai" };
        var provider = new ChatClientProvider(settings);

        // Act
        var result = provider.Provider;

        // Assert
        result.Should().Be("custom-openai");
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Arrange
        var settings = new AIClientSettings { ApiKey = null, ModelId = "gpt-4" };

        // Act
        Action act = () => new ChatClientProvider(settings);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*ApiKey is required*");
    }

    [Fact]
    public void Create_ReturnsChatClient()
    {
        // Arrange
        var settings = new AIClientSettings { ApiKey = "test-key", ModelId = "gpt-4" };
        var provider = new ChatClientProvider(settings);

        // Act
        var client = provider.Create();

        // Assert
        client.Should().NotBeNull();
        client.Should().BeAssignableTo<IChatClient>();
    }

    [Fact]
    public void Create_WithModel_ReturnsChatClient()
    {
        // Arrange
        var settings = new AIClientSettings { ApiKey = "test-key", ModelId = "gpt-4" };
        var provider = new ChatClientProvider(settings);

        // Act
        var client = provider.Create("gpt-3.5-turbo");

        // Assert
        client.Should().NotBeNull();
        client.Should().BeAssignableTo<IChatClient>();
    }
}