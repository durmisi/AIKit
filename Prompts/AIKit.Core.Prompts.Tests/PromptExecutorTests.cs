using AIKit.Prompts.Handlebars;
using AIKit.Prompts.Liquid;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using Microsoft.SemanticKernel.PromptTemplates.Liquid;
using Moq;
using Shouldly;

namespace AIKit.Core.Prompts.Tests;

public class PromptExecutorTests
{
    private readonly Mock<IChatClient> _mockChatClient;

    public PromptExecutorTests()
    {
        _mockChatClient = new Mock<IChatClient>();
        // Setup mock to return a simple response
        _mockChatClient.Setup(c => c.GetResponseAsync(It.IsAny<IList<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatResponse(new ChatMessage(ChatRole.Assistant, "Mock response")));
        _mockChatClient.Setup(c => c.GetStreamingResponseAsync(It.IsAny<IList<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()))
            .Returns(new[] { new ChatResponseUpdate(ChatRole.Assistant, "Mock") }.ToAsyncEnumerable());
    }

    [Fact]
    public async Task LiquidPromptExecutor_HandlesMissingVariables()
    {
        // Arrange
        var executor = new LiquidPromptExecutor(_mockChatClient.Object);
        var template = "Hello {{name}}, your age is {{age}}.";
        var arguments = new KernelArguments { { "name", "John" } }; // Missing 'age'

        // To test template rendering, create the function and render without invoking AI
        var config = new PromptTemplateConfig
        {
            Name = "Test",
            Template = template,
            TemplateFormat = "liquid",
        };
        var kernel = executor.GetType().GetField("_kernel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(executor) as Kernel;
        var factory = new LiquidPromptTemplateFactory();
        var templateObj = factory.Create(config);
        var rendered = await templateObj.RenderAsync(kernel, arguments);

        // Assert
        rendered.ShouldContain("Hello John");
        rendered.ShouldContain("your age is"); // Missing variable rendered as empty
    }

    [Fact]
    public async Task HandlebarsPromptExecutor_HandlesMissingVariables()
    {
        // Arrange
        var executor = new HandlebarsPromptExecutor(_mockChatClient.Object);
        var template = "Hello {{name}}, your age is {{age}}.";
        var arguments = new KernelArguments { { "name", "John" } }; // Missing 'age'

        // To test template rendering, create the function and render without invoking AI
        var config = new PromptTemplateConfig
        {
            Name = "Test",
            Template = template,
            TemplateFormat = "handlebars",
        };
        var kernel = executor.GetType().GetField("_kernel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(executor) as Kernel;
        var factory = new HandlebarsPromptTemplateFactory();
        var templateObj = factory.Create(config);
        var rendered = await templateObj.RenderAsync(kernel, arguments);

        // Assert
        rendered.ShouldContain("Hello John");
        rendered.ShouldContain("your age is"); // Missing variable rendered as empty
    }

    [Fact]
    public async Task PromptExecutor_ThrowsForUnsupportedTemplateType()
    {
        // Arrange
        var executors = new List<IPromptExecutor>
        {
            new LiquidPromptExecutor(_mockChatClient.Object)
        };
        var promptExecutor = new PromptExecutor(executors);

        // Act & Assert
        await Should.ThrowAsync<NotSupportedException>(() =>
            promptExecutor.ExecuteAsync("unsupported", "template", new KernelArguments()));
    }

    [Fact]
    public async Task PromptExecutor_ExecuteAsync_WithValidTemplate()
    {
        // Arrange
        var executors = new List<IPromptExecutor>
        {
            new LiquidPromptExecutor(_mockChatClient.Object)
        };
        var promptExecutor = new PromptExecutor(executors);

        // Act
        var result = await promptExecutor.ExecuteAsync("liquid", "Hello {{name}}", new KernelArguments { { "name", "World" } });

        // Assert
        result.ShouldBe("Mock response"); // Since it invokes AI
    }

    [Fact]
    public async Task LiquidPromptExecutor_ExecuteAsync_WithAllVariables()
    {
        // Arrange
        var executor = new LiquidPromptExecutor(_mockChatClient.Object);
        var template = "Hello {{name}}, your age is {{age}}.";
        var arguments = new KernelArguments { { "name", "John" }, { "age", "30" } };

        // Act
        var result = await executor.ExecuteAsync(template, arguments);

        // Assert
        result.ShouldBe("Mock response");
    }

    [Fact]
    public async Task HandlebarsPromptExecutor_ExecuteAsync_WithAllVariables()
    {
        // Arrange
        var executor = new HandlebarsPromptExecutor(_mockChatClient.Object);
        var template = "Hello {{name}}, your age is {{age}}.";
        var arguments = new KernelArguments { { "name", "John" }, { "age", "30" } };

        // Act
        var result = await executor.ExecuteAsync(template, arguments);

        // Assert
        result.ShouldBe("Mock response");
    }

    [Fact]
    public async Task LiquidPromptExecutor_ThrowsOnInvalidTemplate()
    {
        // Arrange
        var executor = new LiquidPromptExecutor(_mockChatClient.Object);
        var template = ""; // Empty template
        var arguments = new KernelArguments();

        // Act & Assert
        await Should.ThrowAsync<ArgumentException>(() =>
            executor.ExecuteAsync(template, arguments));
    }

    [Fact]
    public async Task LiquidPromptExecutor_ThrowsOnNullArguments()
    {
        // Arrange
        var executor = new LiquidPromptExecutor(_mockChatClient.Object);
        var template = "Hello {{name}}";

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(() =>
            executor.ExecuteAsync(template, null!));
    }

    [Fact]
    public async Task PromptExecutor_Constructor_ThrowsOnNullExecutors()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new PromptExecutor(null!));
    }

    [Fact]
    public async Task PromptExecutor_ExecuteAsync_ThrowsOnNullTemplateType()
    {
        // Arrange
        var executors = new List<IPromptExecutor>
        {
            new LiquidPromptExecutor(_mockChatClient.Object)
        };
        var promptExecutor = new PromptExecutor(executors);

        // Act & Assert
        await Should.ThrowAsync<ArgumentException>(() =>
            promptExecutor.ExecuteAsync(null!, "template", new KernelArguments()));
    }

    [Fact]
    public async Task LiquidPromptExecutor_ExecuteStreamingAsync()
    {
        // Arrange
        var executor = new LiquidPromptExecutor(_mockChatClient.Object);
        var template = "Hello {{name}}";
        var arguments = new KernelArguments { { "name", "World" } };

        // Act
        var stream = executor.ExecuteStreamingAsync(template, arguments);
        var results = await stream.ToListAsync();

        // Assert
        results.ShouldNotBeEmpty();
    }
}