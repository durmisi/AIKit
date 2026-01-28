using AIKit.Clients.AzureOpenAI;
using AIKit.Clients.Resilience;
using AIKit.Clients.Settings;
using Microsoft.Extensions.AI;
using Shouldly;
using Xunit;

namespace AIKit.Clients.Tests.AzureOpenAI;

public class EmbeddingGeneratorFactoryTests
{
    [Fact]
    public void Provider_ReturnsCorrectName()
    {
        // Arrange
        var settings = new Dictionary<string, object> { ["Endpoint"] = "https://example.azure.com", ["ModelId"] = "azure-embed" };
        var factory = new EmbeddingGeneratorFactory(settings);

        // Act
        var result = factory.Provider;

        // Assert
        result.ShouldBe("azure-open-ai");
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Arrange
        var settings = new Dictionary<string, object> { ["Endpoint"] = "not-a-url", ["ModelId"] = "azure-embed" };

        // Act
        Action act = () => new EmbeddingGeneratorFactory(settings);

        // Assert
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldContain("Endpoint must be a valid absolute URI");
    }

    [Fact]
    public void Create_WrapsWithRetryPolicy_WhenProvided()
    {
        // Arrange
        var settings = new Dictionary<string, object>
        {
            ["Endpoint"] = "https://example.azure.com",
            ["ModelId"] = "azure-embed",
            ["RetryPolicy"] = new RetryPolicySettings { MaxRetries = 2 }
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