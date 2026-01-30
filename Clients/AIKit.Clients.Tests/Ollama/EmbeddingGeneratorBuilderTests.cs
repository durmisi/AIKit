using AIKit.Clients.Ollama;
using Microsoft.Extensions.AI;
using Shouldly;
using Xunit;

namespace AIKit.Clients.Tests.Ollama;

public class EmbeddingGeneratorBuilderTests
{
    [Fact]
    public void Provider_ReturnsCorrectName()
    {
        // Arrange
        var builder = new EmbeddingGeneratorBuilder()
            .WithEndpoint("http://localhost:11434")
            .WithModel("nomic-embed-text");

        // Act
        var result = builder.Provider;

        // Assert
        result.ShouldBe("ollama");
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Arrange
        var builder = new EmbeddingGeneratorBuilder()
            .WithModel("nomic-embed-text");

        // Act
        Action act = () => builder.Build();

        // Assert
        act.ShouldThrow<InvalidOperationException>()
            .Message.ShouldContain("Endpoint is required");
    }

    [Fact]
    public void Create_ReturnsEmbeddingGenerator()
    {
        // Arrange
        var builder = new EmbeddingGeneratorBuilder()
            .WithEndpoint("http://localhost:11434")
            .WithModel("nomic-embed-text");

        // Act
        var generator = builder.Build();

        // Assert
        generator.ShouldNotBeNull();
        generator.ShouldBeAssignableTo<IEmbeddingGenerator<string, Embedding<float>>>();
    }
}