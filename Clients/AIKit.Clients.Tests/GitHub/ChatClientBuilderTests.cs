using AIKit.Clients.GitHub;
using Microsoft.Extensions.AI;
using Shouldly;
using Xunit;

namespace AIKit.Clients.Tests.GitHub;

public class ChatClientBuilderTests
{
    [Fact]
    public void Provider_ReturnsCorrectName()
    {
        // Arrange
        var builder = new AIKit.Clients.GitHub.ChatClientBuilder()
            .WithGitHubToken("test")
            .WithModel("gpt-4o");

        // Act
        var result = builder.Provider;

        // Assert
        result.ShouldBe("github-models");
    }

    [Fact]
    public void Create_Throws_WhenSettingsInvalid()
    {
        // Arrange
        var builder = new AIKit.Clients.GitHub.ChatClientBuilder()
            .WithModel("gpt-4o");

        // Act
        Action act = () => builder.Build();

        // Assert
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldContain("GitHubToken is required");
    }

    [Fact]
    public void Create_ReturnsChatClient()
    {
        // Arrange
        var builder = new AIKit.Clients.GitHub.ChatClientBuilder()
            .WithGitHubToken("test-token")
            .WithModel("gpt-4o");

        // Act
        var client = builder.Build();

        // Assert
        client.ShouldNotBeNull();
        client.ShouldBeAssignableTo<IChatClient>();
    }
}