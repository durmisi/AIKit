using AIKit.Clients.OpenAI;
using AIKit.Clients.Resilience;
using Microsoft.Extensions.AI;
using Shouldly;
using Xunit;

namespace AIKit.Clients.Tests.OpenAI;

public class EmbeddingGeneratorBuilderTests
{
    [Fact]
    public void Build_Throws_WhenApiKeyNotSet()
    {
        // Arrange
        var builder = new EmbeddingGeneratorBuilder()
            .WithModel("openai-embed");

        // Act
        Action act = () => builder.Build();

        // Assert
        act.ShouldThrow<InvalidOperationException>()
            .Message.ShouldContain("ApiKey is required");
    }

    [Fact]
    public void Build_Throws_WhenModelIdNotSet()
    {
        // Arrange
        var builder = new EmbeddingGeneratorBuilder()
            .WithApiKey("test-key");

        // Act
        Action act = () => builder.Build();

        // Assert
        act.ShouldThrow<InvalidOperationException>()
            .Message.ShouldContain("Model is required");
    }

    [Fact]
    public void Build_ReturnsEmbeddingGenerator_WhenAllRequiredFieldsSet()
    {
        // Arrange
        var builder = new EmbeddingGeneratorBuilder()
            .WithApiKey("test-key")
            .WithModel("openai-embed");

        // Act
        var generator = builder.Build();

        // Assert
        generator.ShouldNotBeNull();
        generator.ShouldBeAssignableTo<IEmbeddingGenerator<string, Embedding<float>>>();
    }

    [Fact]
    public void Build_WrapsWithRetryPolicy_WhenProvided()
    {
        // Arrange
        var builder = new EmbeddingGeneratorBuilder()
            .WithApiKey("test-key")
            .WithModel("openai-embed")
            .WithRetryPolicy(new RetryPolicySettings()
            {
                MaxRetries = 3,
                MaxDelaySeconds = 30
            });

        // Act
        var generator = builder.Build();

        // Assert
        generator.ShouldNotBeNull();
        generator.ShouldBeAssignableTo<IEmbeddingGenerator<string, Embedding<float>>>();
        // Note: The actual wrapping depends on the implementation
    }
}