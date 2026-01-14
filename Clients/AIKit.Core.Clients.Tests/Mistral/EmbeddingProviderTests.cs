using AIKit.Clients.Mistral;
using AIKit.Core.Clients;
using Shouldly;
using Microsoft.Extensions.AI;
using Xunit;

namespace AIKit.Core.Clients.Tests.Mistral;

public class EmbeddingProviderTests
{
    [Fact]
    public void Provider_ReturnsCorrectName()
    {
        // Arrange
        var settings = new AIClientSettings { ApiKey = "test", ModelId = "mistral-embed" };
        var provider = new EmbeddingProvider(settings);

        // Act
        var result = provider.Provider;

        // Assert
        result.ShouldBe("mistral");
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Arrange
        var settings = new AIClientSettings { ApiKey = null, ModelId = "mistral-embed" };

        // Act
        Action act = () => new EmbeddingProvider(settings);

        // Assert
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldContain("ApiKey is required");
    }

    [Fact]
    public void Create_ReturnsEmbeddingGenerator()
    {
        // Arrange
        var settings = new AIClientSettings { ApiKey = "test-key", ModelId = "mistral-embed" };
        var provider = new EmbeddingProvider(settings);

        // Act
        var generator = provider.Create();

        // Assert
        generator.ShouldNotBeNull();
        generator.ShouldBeAssignableTo<IEmbeddingGenerator<string, Embedding<float>>>();
    }
}









