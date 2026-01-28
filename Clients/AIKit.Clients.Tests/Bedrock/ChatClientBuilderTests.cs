using AIKit.Clients.Bedrock;
using AIKit.Clients.Settings;
using Microsoft.Extensions.AI;
using Shouldly;
using Xunit;

namespace AIKit.Clients.Tests.Bedrock;

public class ChatClientBuilderTests
{
    [Fact]
    public void Provider_ReturnsCorrectName()
    {
        // Arrange
        var builder = new AIKit.Clients.Bedrock.ChatClientBuilder()
            .WithAwsAccessKey("test")
            .WithAwsSecretKey("test")
            .WithAwsRegion("us-east-1")
            .WithModel("anthropic.claude-v2");

        // Act
        var result = builder.Provider;

        // Assert
        result.ShouldBe("aws-bedrock");
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Arrange
        var builder = new AIKit.Clients.Bedrock.ChatClientBuilder()
            .WithAwsSecretKey("test")
            .WithAwsRegion("us-east-1")
            .WithModel("anthropic.claude-v2");

        // Act
        Action act = () => builder.Create();

        // Assert
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldContain("AwsAccessKey is required");
    }

    [Fact]
    public void Create_ReturnsChatClient()
    {
        // Arrange
        var builder = new AIKit.Clients.Bedrock.ChatClientBuilder()
            .WithAwsAccessKey("test-key")
            .WithAwsSecretKey("test-secret")
            .WithAwsRegion("us-east-1")
            .WithModel("anthropic.claude-v2");

        // Act
        var client = builder.Create();

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
        var settings = new AIClientSettings { AwsAccessKey = "test", AwsSecretKey = "test", AwsRegion = "us-east-1", ModelId = "amazon.titan-embed-text-v1" };
        var factory = new EmbeddingGeneratorFactory(settings);

        // Act
        var result = factory.Provider;

        // Assert
        result.ShouldBe("aws-bedrock");
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Arrange
        var settings = new AIClientSettings { AwsAccessKey = null, AwsSecretKey = "test", AwsRegion = "us-east-1", ModelId = "amazon.titan-embed-text-v1" };

        // Act
        Action act = () => new EmbeddingGeneratorFactory(settings);

        // Assert
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldContain("AwsAccessKey is required");
    }

    [Fact]
    public void Create_ReturnsEmbeddingGenerator()
    {
        // Arrange
        var settings = new AIClientSettings { AwsAccessKey = "test-key", AwsSecretKey = "test-secret", AwsRegion = "us-east-1", ModelId = "amazon.titan-embed-text-v1" };
        var factory = new EmbeddingGeneratorFactory(settings);

        // Act
        var generator = factory.Create();

        // Assert
        generator.ShouldNotBeNull();
        generator.ShouldBeAssignableTo<IEmbeddingGenerator<string, Embedding<float>>>();
    }
}