using AIKit.Clients.Ollama;
using AIKit.Core.Clients;
using Shouldly;
using Microsoft.Extensions.AI;
using Xunit;

namespace AIKit.Core.Clients.Tests.Ollama;

public class ChatClientFactoryTests
{
    [Fact]
    public void Provider_ReturnsCorrectName()
    {
        // Arrange
        var settings = new AIClientSettings { Endpoint = "http://localhost:11434", ModelId = "llama3.2" };
        var factory = new ChatClientFactory(settings);

        // Act
        var result = factory.Provider;

        // Assert
        result.ShouldBe("ollama");
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Arrange
        var settings = new AIClientSettings { Endpoint = null, ModelId = "llama3.2" };

        // Act
        Action act = () => new ChatClientFactory(settings);

        // Assert
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldContain("Endpoint is required");
    }

    [Fact]
    public void Create_ReturnsChatClient()
    {
        // Arrange
        var settings = new AIClientSettings { Endpoint = "http://localhost:11434", ModelId = "llama3.2" };
        var factory = new ChatClientFactory(settings);

        // Act
        var client = factory.Create();

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
        var settings = new AIClientSettings { Endpoint = "http://localhost:11434", ModelId = "nomic-embed-text" };
        var factory = new EmbeddingGenerator(settings);

        // Act
        var result = factory.Provider;

        // Assert
        result.ShouldBe("ollama");
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Arrange
        var settings = new AIClientSettings { Endpoint = null, ModelId = "nomic-embed-text" };

        // Act
        Action act = () => new EmbeddingGenerator(settings);

        // Assert
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldContain("Endpoint is required");
    }

    [Fact]
    public void Create_ReturnsEmbeddingGenerator()
    {
        // Arrange
        var settings = new AIClientSettings { Endpoint = "http://localhost:11434", ModelId = "nomic-embed-text" };
        var factory = new EmbeddingGenerator(settings);

        // Act
        var generator = factory.Create();

        // Assert
        generator.ShouldNotBeNull();
        generator.ShouldBeAssignableTo<IEmbeddingGenerator<string, Embedding<float>>>();
    }
}









