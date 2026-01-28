using AIKit.Clients.AzureOpenAI;
using AIKit.Clients.Settings;
using Microsoft.Extensions.AI;
using Shouldly;
using Xunit;

namespace AIKit.Clients.Tests.AzureOpenAI;

public class ChatClientBuilderTests
{
    [Fact]
    public void Provider_ReturnsCorrectName()
    {
        // Arrange
        var builder = new AIKit.Clients.AzureOpenAI.ChatClientBuilder()
            .WithApiKey("test")
            .WithEndpoint("https://test.openai.azure.com")
            .WithModel("gpt-4");

        // Act
        var result = ((AIKit.Clients.Interfaces.IChatClientFactory)builder).Provider;

        // Assert
        result.ShouldBe("azure-open-ai");
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Arrange
        var builder = new AIKit.Clients.AzureOpenAI.ChatClientBuilder()
            .WithEndpoint("https://test.openai.azure.com")
            .WithModel("gpt-4");

        // Act
        Action act = () => ((AIKit.Clients.Interfaces.IChatClientFactory)builder).Create();

        // Assert
        act.ShouldThrow<ArgumentException>().Message.ShouldContain("ApiKey is required");
    }

    [Fact]
    public void Create_ReturnsChatClient()
    {
        // Arrange
        var builder = new AIKit.Clients.AzureOpenAI.ChatClientBuilder()
            .WithApiKey("test-key")
            .WithEndpoint("https://test.openai.azure.com")
            .WithModel("gpt-4");

        // Act
        var client = ((AIKit.Clients.Interfaces.IChatClientFactory)builder).Create();

        // Assert
        client.ShouldNotBeNull();
        client.ShouldBeAssignableTo<IChatClient>();
    }
}

public class EmbeddingGeneratorFactoryTests
{
    [Fact]
    public void Provider_ReturnsCorrectName()
    {
        // Arrange
        var settings = new AIClientSettings { ApiKey = "test", Endpoint = "https://test.openai.azure.com", ModelId = "text-embedding-ada-002" };
        var factory = new EmbeddingGeneratorFactory(settings);

        // Act
        var result = factory.Provider;

        // Assert
        result.ShouldBe("azure-open-ai");
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Arrange
        var settings = new AIClientSettings { ApiKey = null, Endpoint = "https://test.openai.azure.com", ModelId = "text-embedding-ada-002" };

        // Act
        Action act = () => new EmbeddingGeneratorFactory(settings);

        // Assert
        act.ShouldThrow<ArgumentException>().Message.ShouldContain("ApiKey is required");
    }

    [Fact]
    public void Create_ReturnsEmbeddingGenerator()
    {
        // Arrange
        var settings = new AIClientSettings { ApiKey = "test-key", Endpoint = "https://test.openai.azure.com", ModelId = "text-embedding-ada-002" };
        var factory = new EmbeddingGeneratorFactory(settings);

        // Act
        var generator = factory.Create();

        // Assert
        generator.ShouldNotBeNull();
        generator.ShouldBeAssignableTo<IEmbeddingGenerator<string, Embedding<float>>>();
    }
}