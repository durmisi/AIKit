using AIKit.Clients.OpenAI;
using AIKit.Clients.Resilience;
using AIKit.Clients.Settings;
using Microsoft.Extensions.AI;
using Shouldly;
using Xunit;

namespace AIKit.Clients.Tests.OpenAI;

public class EmbeddingGeneratorFactoryTests
{
    [Fact]
    public void Provider_ReturnsCorrectName()
    {
        // Arrange
        var settings = new Dictionary<string, object> { ["ApiKey"] = "test", ["ModelId"] = "openai-embed" };
        var factory = new EmbeddingGeneratorFactory(settings);

        // Act
        var result = factory.Provider;

        // Assert
        result.ShouldBe("open-ai");
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Arrange
        var settings = new Dictionary<string, object> { ["ApiKey"] = null, ["ModelId"] = "openai-embed" };

        // Act
        Action act = () => new EmbeddingGeneratorFactory(settings);

        // Assert
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldContain("ApiKey is required");
    }

    [Fact]
    public void Create_WrapsWithRetryPolicy_WhenProvided()
    {
        // Arrange
        var settings = new Dictionary<string, object>
        {
            ["ApiKey"] = "test-key",
            ["ModelId"] = "openai-embed",
            ["RetryPolicy"] = new RetryPolicySettings { MaxRetries = 1 }
        };

        var factory = new EmbeddingGeneratorFactory(settings);

        // Act
        var generator = factory.Create(settings);

        // Assert
        generator.ShouldNotBeNull();
        generator.ShouldBeAssignableTo<IEmbeddingGenerator<string, Embedding<float>>>();
        generator.ShouldBeAssignableTo<AIKit.Clients.Resilience.RetryEmbeddingGenerator>();
    }
}