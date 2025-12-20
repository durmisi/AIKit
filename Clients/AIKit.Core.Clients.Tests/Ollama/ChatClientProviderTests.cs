using AIKit.Clients.Ollama;
using AIKit.Core.Clients;
using FluentAssertions;
using Microsoft.Extensions.AI;
using Xunit;

namespace AIKit.Core.Clients.Tests.Ollama;

public class ChatClientProviderTests
{
    [Fact]
    public void Provider_ReturnsCorrectName()
    {
        // Arrange
        var settings = new AIClientSettings { Endpoint = "http://localhost:11434", ModelId = "llama3.2" };
        var provider = new ChatClientProvider(settings);

        // Act
        var result = provider.Provider;

        // Assert
        result.Should().Be("ollama");
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Arrange
        var settings = new AIClientSettings { Endpoint = null, ModelId = "llama3.2" };

        // Act
        Action act = () => new ChatClientProvider(settings);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Endpoint is required*");
    }

    [Fact]
    public void Create_ReturnsChatClient()
    {
        // Arrange
        var settings = new AIClientSettings { Endpoint = "http://localhost:11434", ModelId = "llama3.2" };
        var provider = new ChatClientProvider(settings);

        // Act
        var client = provider.Create();

        // Assert
        client.Should().NotBeNull();
        client.Should().BeAssignableTo<IChatClient>();
    }
}

public class EmbeddingGeneratorTests
{
    [Fact]
    public void Provider_ReturnsCorrectName()
    {
        // Arrange
        var settings = new AIClientSettings { Endpoint = "http://localhost:11434", ModelId = "nomic-embed-text" };
        var provider = new EmbeddingGenerator(settings);

        // Act
        var result = provider.Provider;

        // Assert
        result.Should().Be("ollama");
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Arrange
        var settings = new AIClientSettings { Endpoint = null, ModelId = "nomic-embed-text" };

        // Act
        Action act = () => new EmbeddingGenerator(settings);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Endpoint is required*");
    }

    [Fact]
    public void Create_ReturnsEmbeddingGenerator()
    {
        // Arrange
        var settings = new AIClientSettings { Endpoint = "http://localhost:11434", ModelId = "nomic-embed-text" };
        var provider = new EmbeddingGenerator(settings);

        // Act
        var generator = provider.Create();

        // Assert
        generator.Should().NotBeNull();
        generator.Should().BeAssignableTo<IEmbeddingGenerator<string, Embedding<float>>>();
    }
}