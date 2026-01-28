using AIKit.Clients.Ollama;
using Microsoft.Extensions.AI;
using Shouldly;
using Xunit;

namespace AIKit.Clients.Tests.Ollama;

public class ChatClientBuilderTests
{
    [Fact]
    public void Provider_ReturnsCorrectName()
    {
        // Arrange
        var builder = new AIKit.Clients.Ollama.ChatClientBuilder()
            .WithEndpoint("http://localhost:11434")
            .WithModel("llama3.2");

        // Act
        var result = builder.Provider;

        // Assert
        result.ShouldBe("ollama");
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Arrange
        var builder = new AIKit.Clients.Ollama.ChatClientBuilder()
            .WithModel("llama3.2");

        // Act
        Action act = () => builder.Build();

        // Assert
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldContain("Endpoint is required");
    }

    [Fact]
    public void Create_ReturnsChatClient()
    {
        // Arrange
        var builder = new AIKit.Clients.Ollama.ChatClientBuilder()
            .WithEndpoint("http://localhost:11434")
            .WithModel("llama3.2");

        // Act
        var client = builder.Build();

        // Assert
        client.ShouldNotBeNull();
        client.ShouldBeAssignableTo<IChatClient>();
    }
}

public class EmbeddingGeneratorBuilderTests
{
    [Fact]
    public void Provider_ReturnsCorrectName()
    {
        // Arrange
        var builder = new EmbeddingGeneratorBuilder()
            .WithEndpoint("http://localhost:11434")
            .WithModelId("nomic-embed-text");

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
            .WithModelId("nomic-embed-text");

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
            .WithModelId("nomic-embed-text");

        // Act
        var generator = builder.Build();

        // Assert
        generator.ShouldNotBeNull();
        generator.ShouldBeAssignableTo<IEmbeddingGenerator<string, Embedding<float>>>();
    }
}