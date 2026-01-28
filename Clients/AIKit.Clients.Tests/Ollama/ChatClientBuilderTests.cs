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

public class EmbeddingGeneratorTests
{
    [Fact]
    public void Provider_ReturnsCorrectName()
    {
        // Arrange
        var settings = new Dictionary<string, object> { ["Endpoint"] = "http://localhost:11434", ["ModelId"] = "nomic-embed-text" };
        var factory = new EmbeddingGeneratorFactory(settings);

        // Act
        var result = factory.Provider;

        // Assert
        result.ShouldBe("ollama");
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Arrange
        var settings = new Dictionary<string, object> { ["Endpoint"] = null, ["ModelId"] = "nomic-embed-text" };

        // Act
        Action act = () => new EmbeddingGeneratorFactory(settings);

        // Assert
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldContain("Endpoint is required");
    }

    [Fact]
    public void Create_ReturnsEmbeddingGenerator()
    {
        // Arrange
        var settings = new Dictionary<string, object> { ["Endpoint"] = "http://localhost:11434", ["ModelId"] = "nomic-embed-text" };
        var factory = new EmbeddingGeneratorFactory(settings);

        // Act
        var generator = factory.Create();

        // Assert
        generator.ShouldNotBeNull();
        generator.ShouldBeAssignableTo<IEmbeddingGenerator<string, Embedding<float>>>();
    }
}