using AIKit.Core.Prompts;
using AIKit.Prompts.Handlebars;
using AIKit.Prompts.Liquid;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using Microsoft.SemanticKernel.PromptTemplates.Liquid;
using Moq;

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
        Assert.Contains("Hello John", rendered);
        Assert.Contains("your age is", rendered); // Missing variable rendered as empty
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
        Assert.Contains("Hello John", rendered);
        Assert.Contains("your age is", rendered); // Missing variable rendered as empty
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
        await Assert.ThrowsAsync<NotSupportedException>(() =>
            promptExecutor.ExecuteAsync("unsupported", "template", new KernelArguments()));
    }
}