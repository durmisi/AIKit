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
        var builder = new EmbeddingGeneratorBuilder()
            .WithGitHubToken("test")
            .WithModel("text-embedding-3-small");

        // Act
        var result = builder.Provider;

        // Assert
        result.ShouldBe("github-models");
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Act
        Action act = () => new EmbeddingGeneratorBuilder()
        .WithModel("text-embedding-3-small")
        .Build();

        // Assert
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldContain("GitHubToken is required");
    }

    [Fact]
    public void Create_ReturnsEmbeddingGenerator()
    {
        // Arrange
        var builder = new EmbeddingGeneratorBuilder()
            .WithGitHubToken("test-token")
            .WithModel("text-embedding-3-small");

        // Act
        var generator = builder.Build();

        // Assert
        generator.ShouldNotBeNull();
        generator.ShouldBeAssignableTo<IEmbeddingGenerator<string, Embedding<float>>>();
    }
}