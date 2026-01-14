using AIKit.Core.Clients;
using Shouldly;
using Xunit;

namespace AIKit.Core.Clients.Tests;

public class AIClientSettingsValidatorTests
{
    [Fact]
    public void RequireApiKey_Throws_WhenApiKeyIsNull()
    {
        // Arrange
        var settings = new AIClientSettings { ApiKey = null };

        // Act
        Action act = () => AIClientSettingsValidator.RequireApiKey(settings);

        // Assert
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldContain("ApiKey is required");
    }

    [Fact]
    public void RequireApiKey_Throws_WhenApiKeyIsEmpty()
    {
        // Arrange
        var settings = new AIClientSettings { ApiKey = string.Empty };

        // Act
        Action act = () => AIClientSettingsValidator.RequireApiKey(settings);

        // Assert
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldContain("ApiKey is required");
    }

    [Fact]
    public void RequireApiKey_DoesNotThrow_WhenApiKeyIsSet()
    {
        // Arrange
        var settings = new AIClientSettings { ApiKey = "test-key" };

        // Act
        Action act = () => AIClientSettingsValidator.RequireApiKey(settings);

        // Assert
        act.ShouldNotThrow();
    }

    [Fact]
    public void RequireEndpoint_Throws_WhenEndpointIsNull()
    {
        // Arrange
        var settings = new AIClientSettings { Endpoint = null };

        // Act
        Action act = () => AIClientSettingsValidator.RequireEndpoint(settings);

        // Assert
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldContain("Endpoint is required");
    }

    [Fact]
    public void RequireModel_Throws_WhenModelIdIsNull()
    {
        // Arrange
        var settings = new AIClientSettings { ModelId = null };

        // Act
        Action act = () => AIClientSettingsValidator.RequireModel(settings);

        // Assert
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldContain("ModelId is required");
    }
}









