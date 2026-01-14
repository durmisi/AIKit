using AIKit.Clients.AzureOpenAI;
using AIKit.Core.Clients;
using Shouldly;
using Microsoft.Extensions.AI;
using Xunit;

namespace AIKit.Core.Clients.Tests.AzureOpenAI;

public class ChatClientProviderTests
{
    [Fact]
    public void Provider_ReturnsCorrectName()
    {
        // Arrange
        var settings = new AIClientSettings { ApiKey = "test", Endpoint = "https://test.openai.azure.com", ModelId = "gpt-4" };
        var provider = new ChatClientProvider(settings);

        // Act
        var result = provider.Provider;

        // Assert
        result.ShouldBe("azure-open-ai");
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Arrange
        var settings = new AIClientSettings { ApiKey = null, Endpoint = "https://test.openai.azure.com", ModelId = "gpt-4" };

        // Act
        Action act = () => new ChatClientProvider(settings);

        // Assert
        act.ShouldThrow<ArgumentException>().Message.ShouldContain("ApiKey is required");
    }

    [Fact]
    public void Create_ReturnsChatClient()
    {
        // Arrange
        var settings = new AIClientSettings { ApiKey = "test-key", Endpoint = "https://test.openai.azure.com", ModelId = "gpt-4" };
        var provider = new ChatClientProvider(settings);

        // Act
        var client = provider.Create();

        // Assert
        client.ShouldNotBeNull();
        client.ShouldBeAssignableTo<IChatClient>();
    }
}

public class EmbeddingProviderTests
{
    [Fact]
    public void Provider_ReturnsCorrectName()
    {
        // Arrange
        var settings = new AIClientSettings { ApiKey = "test", Endpoint = "https://test.openai.azure.com", ModelId = "text-embedding-ada-002" };
        var provider = new EmbeddingProvider(settings);

        // Act
        var result = provider.Provider;

        // Assert
        result.ShouldBe("azure-open-ai");
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Arrange
        var settings = new AIClientSettings { ApiKey = null, Endpoint = "https://test.openai.azure.com", ModelId = "text-embedding-ada-002" };

        // Act
        Action act = () => new EmbeddingProvider(settings);

        // Assert
        act.ShouldThrow<ArgumentException>().Message.ShouldContain("ApiKey is required");
    }

    [Fact]
    public void Create_ReturnsEmbeddingGenerator()
    {
        // Arrange
        var settings = new AIClientSettings { ApiKey = "test-key", Endpoint = "https://test.openai.azure.com", ModelId = "text-embedding-ada-002" };
        var provider = new EmbeddingProvider(settings);

        // Act
        var generator = provider.Create();

        // Assert
        generator.ShouldNotBeNull();
        generator.ShouldBeAssignableTo<IEmbeddingGenerator<string, Embedding<float>>>();
    }
}









