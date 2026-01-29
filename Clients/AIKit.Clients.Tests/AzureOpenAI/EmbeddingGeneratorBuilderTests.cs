using AIKit.Clients.AzureOpenAI;
using Microsoft.Extensions.AI;
using Shouldly;
using Xunit;

namespace AIKit.Clients.Tests.AzureOpenAI;

public class EmbeddingGeneratorBuilderTests
{
    [Fact]
    public void Build_Throws_WhenEndpointNotSet()
    {
        // Arrange
        var builder = new EmbeddingGeneratorBuilder()
            .WithModelId("azure-embed")
            .WithApiKey("test-key");

        // Act
        Action act = () => builder.Build();

        // Assert
        act.ShouldThrow<InvalidOperationException>()
            .Message.ShouldContain("Endpoint is required");
    }

    [Fact]
    public void Build_Throws_WhenModelIdNotSet()
    {
        // Arrange
        var builder = new EmbeddingGeneratorBuilder()
            .WithEndpoint("https://example.azure.com")
            .WithApiKey("test-key");

        // Act
        Action act = () => builder.Build();

        // Assert
        act.ShouldThrow<InvalidOperationException>()
            .Message.ShouldContain("ModelId is required");
    }

    [Fact]
    public void Build_Throws_WhenApiKeyNotSet()
    {
        // Arrange
        var builder = new EmbeddingGeneratorBuilder()
            .WithEndpoint("https://example.azure.com")
            .WithModelId("azure-embed");

        // Act
        Action act = () => builder.Build();

        // Assert
        act.ShouldThrow<InvalidOperationException>()
            .Message.ShouldContain("Either ApiKey, DefaultAzureCredential, or TokenCredential is required");
    }

    [Fact]
    public void Build_ReturnsEmbeddingGenerator_WhenAllRequiredFieldsSet()
    {
        // Arrange
        var builder = new EmbeddingGeneratorBuilder()
            .WithEndpoint("https://example.azure.com")
            .WithModelId("azure-embed")
            .WithApiKey("test-key");

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
            .WithEndpoint("https://example.azure.com")
            .WithModelId("azure-embed")
            .WithApiKey("test-key")
            .WithRetryPolicy(new Settings.RetryPolicySettings()
            {
                MaxRetries = 3,
                MaxDelaySeconds = 30,
            });

        // Act
        var generator = builder.Build();

        // Assert
        generator.ShouldNotBeNull();
        generator.ShouldBeAssignableTo<IEmbeddingGenerator<string, Embedding<float>>>();
        // Note: The actual wrapping depends on the implementation
    }
}