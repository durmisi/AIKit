using AIKit.Clients.Mistral;
using Microsoft.Extensions.AI;
using Shouldly;
using Xunit;

namespace AIKit.Clients.Tests.Mistral;

public class EmbeddingGeneratorBuilderTests
{
    [Fact]
    public void Build_Throws_WhenApiKeyNotSet()
    {
        // Arrange
        var builder = new EmbeddingGeneratorBuilder()
            .WithModel("mistral-embed");

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
            .WithModel("mistral-embed");

        // Act
        var generator = builder.Build();

        // Assert
        generator.ShouldNotBeNull();
        generator.ShouldBeAssignableTo<IEmbeddingGenerator<string, Embedding<float>>>();
    }
}