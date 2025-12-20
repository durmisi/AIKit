using AIKit.Clients.GitHub;
using AIKit.Core.Clients;
using FluentAssertions;
using Microsoft.Extensions.AI;
using Xunit;

namespace AIKit.Core.Clients.Tests.GitHub;

public class ChatClientProviderTests
{
    [Fact]
    public void Provider_ReturnsCorrectName()
    {
        // Arrange
        var settings = new AIClientSettings { GitHubToken = "test", ModelId = "gpt-4o" };
        var provider = new ChatClientProvider(settings);

        // Act
        var result = provider.Provider;

        // Assert
        result.Should().Be("github-models");
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Arrange
        var settings = new AIClientSettings { GitHubToken = null, ModelId = "gpt-4o" };

        // Act
        Action act = () => new ChatClientProvider(settings);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*GitHubToken is required*");
    }

    [Fact]
    public void Create_ReturnsChatClient()
    {
        // Arrange
        var settings = new AIClientSettings { GitHubToken = "test-token", ModelId = "gpt-4o" };
        var provider = new ChatClientProvider(settings);

        // Act
        var client = provider.Create();

        // Assert
        client.Should().NotBeNull();
        client.Should().BeAssignableTo<IChatClient>();
    }
}

public class EmbeddingProviderTests
{
    [Fact]
    public void Provider_ReturnsCorrectName()
    {
        // Arrange
        var settings = new AIClientSettings { GitHubToken = "test", ModelId = "text-embedding-3-small" };
        var provider = new EmbeddingProvider(settings);

        // Act
        var result = provider.Provider;

        // Assert
        result.Should().Be("github-models");
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Arrange
        var settings = new AIClientSettings { GitHubToken = null, ModelId = "text-embedding-3-small" };

        // Act
        Action act = () => new EmbeddingProvider(settings);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*GitHubToken is required*");
    }

    [Fact]
    public void Create_ReturnsEmbeddingGenerator()
    {
        // Arrange
        var settings = new AIClientSettings { GitHubToken = "test-token", ModelId = "text-embedding-3-small" };
        var provider = new EmbeddingProvider(settings);

        // Act
        var generator = provider.Create();

        // Assert
        generator.Should().NotBeNull();
        generator.Should().BeAssignableTo<IEmbeddingGenerator<string, Embedding<float>>>();
    }
}