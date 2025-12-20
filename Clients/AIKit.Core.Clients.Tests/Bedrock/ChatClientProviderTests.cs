using AIKit.Clients.Bedrock;
using AIKit.Core.Clients;
using FluentAssertions;
using Microsoft.Extensions.AI;
using Xunit;

namespace AIKit.Core.Clients.Tests.Bedrock;

public class ChatClientProviderTests
{
    [Fact]
    public void Provider_ReturnsCorrectName()
    {
        // Arrange
        var settings = new AIClientSettings { AwsAccessKey = "test", AwsSecretKey = "test", AwsRegion = "us-east-1", ModelId = "anthropic.claude-v2" };
        var provider = new ChatClientProvider(settings);

        // Act
        var result = provider.Provider;

        // Assert
        result.Should().Be("aws-bedrock");
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Arrange
        var settings = new AIClientSettings { AwsAccessKey = null, AwsSecretKey = "test", AwsRegion = "us-east-1", ModelId = "anthropic.claude-v2" };

        // Act
        Action act = () => new ChatClientProvider(settings);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*AwsAccessKey is required*");
    }

    [Fact]
    public void Create_ReturnsChatClient()
    {
        // Arrange
        var settings = new AIClientSettings { AwsAccessKey = "test-key", AwsSecretKey = "test-secret", AwsRegion = "us-east-1", ModelId = "anthropic.claude-v2" };
        var provider = new ChatClientProvider(settings);

        // Act
        var client = provider.Create();

        // Assert
        client.Should().NotBeNull();
        client.Should().BeAssignableTo<IChatClient>();
    }
}

public class EmbeddingProviderTests
{
    [Fact]
    public void Provider_ReturnsCorrectName()
    {
        // Arrange
        var settings = new AIClientSettings { AwsAccessKey = "test", AwsSecretKey = "test", AwsRegion = "us-east-1", ModelId = "amazon.titan-embed-text-v1" };
        var provider = new EmbeddingProvider(settings);

        // Act
        var result = provider.Provider;

        // Assert
        result.Should().Be("aws-bedrock");
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Arrange
        var settings = new AIClientSettings { AwsAccessKey = null, AwsSecretKey = "test", AwsRegion = "us-east-1", ModelId = "amazon.titan-embed-text-v1" };

        // Act
        Action act = () => new EmbeddingProvider(settings);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*AwsAccessKey is required*");
    }

    [Fact]
    public void Create_ReturnsEmbeddingGenerator()
    {
        // Arrange
        var settings = new AIClientSettings { AwsAccessKey = "test-key", AwsSecretKey = "test-secret", AwsRegion = "us-east-1", ModelId = "amazon.titan-embed-text-v1" };
        var provider = new EmbeddingProvider(settings);

        // Act
        var generator = provider.Create();

        // Assert
        generator.Should().NotBeNull();
        generator.Should().BeAssignableTo<IEmbeddingGenerator<string, Embedding<float>>>();
    }
}