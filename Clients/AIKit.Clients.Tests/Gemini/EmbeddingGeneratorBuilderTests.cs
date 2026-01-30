using AIKit.Clients.Gemini;
using Microsoft.Extensions.AI;
using Shouldly;
using Xunit;

namespace AIKit.Clients.Tests.Gemini;

public class EmbeddingGeneratorBuilderTests
{
    [Fact]
    public void Build_Throws_WhenApiKeyNotSet()
    {
        // Arrange
        var builder = new EmbeddingGeneratorBuilder()
            .WithModel("text-embedding-004");

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
            .WithModel("text-embedding-004");

        // Act
        var generator = builder.Build();

        // Assert
        generator.ShouldNotBeNull();
        generator.ShouldBeAssignableTo<IEmbeddingGenerator<string, Embedding<float>>>();
    }
}