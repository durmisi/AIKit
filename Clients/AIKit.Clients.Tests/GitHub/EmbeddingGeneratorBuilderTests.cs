using AIKit.Clients.GitHub;
using Microsoft.Extensions.AI;
using Shouldly;
using Xunit;

namespace AIKit.Clients.Tests.GitHub;

public class EmbeddingGeneratorBuilderTests
{
    [Fact]
    public void Provider_ReturnsCorrectName()
    {
        // Arrange
        var generator = new EmbeddingGeneratorBuilder()
            .WithGitHubToken("test")
            .WithModelId("text-embedding-3-small");

        // Act
        var result = generator.Build();

        // Assert
        result.ShouldNotBeNull();
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Act
        Action act = () => new EmbeddingGeneratorBuilder()
        .WithModelId("text-embedding-3-small")
        .Build();

        // Assert
        act.ShouldThrow<InvalidOperationException>()
            .Message.ShouldContain("GitHubToken is required");
    }

    [Fact]
    public void Create_ReturnsEmbeddingGenerator()
    {
        // Arrange
        var builder = new EmbeddingGeneratorBuilder()
            .WithGitHubToken("test-token")
            .WithModelId("text-embedding-3-small");

        // Act
        var generator = builder.Build();

        // Assert
        generator.ShouldNotBeNull();
        generator.ShouldBeAssignableTo<IEmbeddingGenerator<string, Embedding<float>>>();
    }
}