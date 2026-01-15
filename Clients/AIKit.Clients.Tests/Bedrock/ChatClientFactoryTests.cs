using AIKit.Clients.Bedrock;
using Microsoft.Extensions.AI;
using Shouldly;
using Xunit;

namespace AIKit.Clients.Tests.Bedrock;

public class ChatClientFactoryTests
{
    [Fact]
    public void Provider_ReturnsCorrectName()
    {
        // Arrange
        var settings = new AIClientSettings { AwsAccessKey = "test", AwsSecretKey = "test", AwsRegion = "us-east-1", ModelId = "anthropic.claude-v2" };
        var factory = new ChatClientFactory(settings);

        // Act
        var result = factory.Provider;

        // Assert
        result.ShouldBe("aws-bedrock");
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Arrange
        var settings = new AIClientSettings { AwsAccessKey = null, AwsSecretKey = "test", AwsRegion = "us-east-1", ModelId = "anthropic.claude-v2" };

        // Act
        Action act = () => new ChatClientFactory(settings);

        // Assert
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldContain("AwsAccessKey is required");
    }

    [Fact]
    public void Create_ReturnsChatClient()
    {
        // Arrange
        var settings = new AIClientSettings { AwsAccessKey = "test-key", AwsSecretKey = "test-secret", AwsRegion = "us-east-1", ModelId = "anthropic.claude-v2" };
        var factory = new ChatClientFactory(settings);

        // Act
        var client = factory.Create();

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









