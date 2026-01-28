using AIKit.Clients.Bedrock;
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
        Action act = () => builder.Build();

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
            .WithAwsAccessKey("test")
            .WithAwsSecretKey("test")
            .WithAwsRegion("us-east-1")
            .WithModelId("amazon.titan-embed-text-v1");

        // Act
        var result = builder.Provider;

        // Assert
        result.ShouldBe("aws-bedrock");
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Arrange

        // Act
        Action act = () => new EmbeddingGeneratorBuilder().WithAwsAccessKey(null);

        // Assert
        act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Create_ReturnsEmbeddingGenerator()
    {
        // Arrange
        var builder = new EmbeddingGeneratorBuilder()
            .WithAwsAccessKey("test-key")
            .WithAwsSecretKey("test-secret")
            .WithAwsRegion("us-east-1")
            .WithModelId("amazon.titan-embed-text-v1");

        // Act
        var generator = builder.Build();

        // Assert
        generator.ShouldNotBeNull();
        generator.ShouldBeAssignableTo<IEmbeddingGenerator<string, Embedding<float>>>();
    }
}