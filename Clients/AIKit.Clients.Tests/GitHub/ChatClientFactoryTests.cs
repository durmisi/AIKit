using AIKit.Clients.GitHub;
using Microsoft.Extensions.AI;
using Shouldly;
using Xunit;

namespace AIKit.Clients.Tests.GitHub;

public class ChatClientFactoryTests
{
    [Fact]
    public void Provider_ReturnsCorrectName()
    {
        // Arrange
        var settings = new AIClientSettings { GitHubToken = "test", ModelId = "gpt-4o" };
        var factory = new ChatClientFactory(settings);

        // Act
        var result = factory.Provider;

        // Assert
        result.ShouldBe("github-models");
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Arrange
        var settings = new AIClientSettings { GitHubToken = null, ModelId = "gpt-4o" };

        // Act
        Action act = () => new ChatClientFactory(settings);

        // Assert
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldContain("GitHubToken is required");
    }

    [Fact]
    public void Create_ReturnsChatClient()
    {
        // Arrange
        var settings = new AIClientSettings { GitHubToken = "test-token", ModelId = "gpt-4o" };
        var factory = new ChatClientFactory(settings);

        // Act
        var client = factory.Create();

        // Assert
        client.ShouldNotBeNull();
        client.ShouldBeAssignableTo<IChatClient>();
    }
}

public class EmbeddingGeneratorFactoryTests
{
    [Fact]
    public void Provider_ReturnsCorrectName()
    {
        // Arrange
        var settings = new AIClientSettings { GitHubToken = "test", ModelId = "text-embedding-3-small" };
        var factory = new EmbeddingGeneratorFactory(settings);

        // Act
        var result = factory.Provider;

        // Assert
        result.ShouldBe("github-models");
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Arrange
        var settings = new AIClientSettings { GitHubToken = null, ModelId = "text-embedding-3-small" };

        // Act
        Action act = () => new EmbeddingGeneratorFactory(settings);

        // Assert
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldContain("GitHubToken is required");
    }

    [Fact]
    public void Create_ReturnsEmbeddingGenerator()
    {
        // Arrange
        var settings = new AIClientSettings { GitHubToken = "test-token", ModelId = "text-embedding-3-small" };
        var factory = new EmbeddingGeneratorFactory(settings);

        // Act
        var generator = factory.Create();

        // Assert
        generator.ShouldNotBeNull();
        generator.ShouldBeAssignableTo<IEmbeddingGenerator<string, Embedding<float>>>();
    }
}









