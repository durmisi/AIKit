using AIKit.Prompts.Handlebars;
using AIKit.Prompts.Liquid;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using Microsoft.SemanticKernel.PromptTemplates.Liquid;
using Moq;
using Shouldly;
using Xunit.Abstractions;

namespace AIKit.Prompts.Tests;

public class PromptExecutorTests
{
    private readonly Mock<IChatClient> _mockChatClient;
    private readonly ITestOutputHelper _output;

    public PromptExecutorTests(ITestOutputHelper output)
    {
        _output = output;
        _mockChatClient = new Mock<IChatClient>();

        _mockChatClient
            .Setup(c => c.GetResponseAsync(
                It.IsAny<IList<ChatMessage>>(),
                It.IsAny<ChatOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatResponse(
                new ChatMessage(ChatRole.Assistant, "Mock response")));

        _mockChatClient
            .Setup(c => c.GetStreamingResponseAsync(
                It.IsAny<IList<ChatMessage>>(),
                It.IsAny<ChatOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(new[]
            {
                new ChatResponseUpdate(ChatRole.Assistant, "Mock "),
                new ChatResponseUpdate(ChatRole.Assistant, "stream")
            }.ToAsyncEnumerable());
    }

    // -------------------------
    // LIQUID TEMPLATE TESTS
    // -------------------------

    [Fact]
    public async Task LiquidPromptExecutor_RendersComplexTemplate_WithLoopsAndConditionals()
    {
        var executor = new Liquid.PromptExecutor(_mockChatClient.Object);

        var template = """
            Hello {{ user.name }}!

            {% if user.isPremium %}
            Thanks for being a premium user.
            {% else %}
            Consider upgrading your plan.
            {% endif %}

            Your orders:
            {% for order in orders %}
            - {{ order.id }}: {{ order.product }} ({{ order.price }})
            {% endfor %}
            """;

        var arguments = new KernelArguments
        {
            ["user"] = new Dictionary<string, object>
            {
                ["name"] = "John",
                ["isPremium"] = false
            },
            ["orders"] = new[]
            {
                new Dictionary<string, object> { ["id"] = "A1", ["product"] = "Laptop", ["price"] = "1200€" },
                new Dictionary<string, object> { ["id"] = "B2", ["product"] = "Mouse", ["price"] = "25€" }
            }
        };

        var kernel = GetKernel(executor);
        var factory = new LiquidPromptTemplateFactory();
        var templateObj = factory.Create(new PromptTemplateConfig
        {
            Name = "ComplexLiquid",
            Template = template,
            TemplateFormat = "liquid",
            AllowDangerouslySetContent = true
        });

        var rendered = await templateObj.RenderAsync(kernel, arguments);

        _output.WriteLine("=== Liquid Template Input ===");
        _output.WriteLine(template);
        _output.WriteLine("=== Arguments ===");
        _output.WriteLine($"user.name: John, user.isPremium: false");
        _output.WriteLine("orders: [A1: Laptop (1200€), B2: Mouse (25€)]");
        _output.WriteLine("=== Rendered Output ===");
        _output.WriteLine(rendered);

        rendered.ShouldContain("Hello John!");
        rendered.ShouldContain("Consider upgrading");
        rendered.ShouldContain("Laptop");
        rendered.ShouldContain("Mouse");
    }

    [Fact]
    public async Task LiquidPromptExecutor_HandlesMissingNestedProperties()
    {
        var executor = new Liquid.PromptExecutor(_mockChatClient.Object);

        var template = """
            User: {{ user.name }}
            City: {{ user.address.city }}
            """;

        var arguments = new KernelArguments
        {
            ["user"] = new Dictionary<string, object>
            {
                ["name"] = "John"
                // address missing
            }
        };

        var kernel = GetKernel(executor);
        var factory = new LiquidPromptTemplateFactory();
        var templateObj = factory.Create(new PromptTemplateConfig
        {
            Name = "LiquidMissingNested",
            Template = template,
            TemplateFormat = "liquid",
            AllowDangerouslySetContent = true
        });

        var rendered = await templateObj.RenderAsync(kernel, arguments);

        _output.WriteLine("=== Liquid Template Input (Missing Nested Properties) ===");
        _output.WriteLine(template);
        _output.WriteLine("=== Arguments ===");
        _output.WriteLine("user.name: John (address missing)");
        _output.WriteLine("=== Rendered Output ===");
        _output.WriteLine(rendered);

        rendered.ShouldContain("User: John");
        rendered.ShouldContain("City:");
    }

    // -------------------------
    // HANDLEBARS TEMPLATE TESTS
    // -------------------------

    [Fact]
    public async Task HandlebarsPromptExecutor_RendersComplexTemplate_WithEachAndIf()
    {
        var executor = new Handlebars.PromptExecutor(_mockChatClient.Object);

        var template = """
            Hello {{user.name}}!

            {{#if user.isPremium}}
            Thanks for being a premium user.
            {{else}}
            Consider upgrading your plan.
            {{/if}}

            Your orders:
            {{#each orders}}
            - {{id}}: {{product}} ({{price}})
            {{/each}}
            """;

        var arguments = new KernelArguments
        {
            ["user"] = new Dictionary<string, object>
            {
                ["name"] = "Jane",
                ["isPremium"] = false
            },
            ["orders"] = new[]
            {
                new Dictionary<string, object> { ["id"] = "X1", ["product"] = "Phone", ["price"] = "800€" },
                new Dictionary<string, object> { ["id"] = "Y2", ["product"] = "Case", ["price"] = "20€" }
            }
        };

        var kernel = GetKernel(executor);
        var factory = new HandlebarsPromptTemplateFactory();
        var templateObj = factory.Create(new PromptTemplateConfig
        {
            Name = "ComplexHandlebars",
            Template = template,
            TemplateFormat = "handlebars",
            AllowDangerouslySetContent = true
        });

        var rendered = await templateObj.RenderAsync(kernel, arguments);

        _output.WriteLine("=== Handlebars Template Input ===");
        _output.WriteLine(template);
        _output.WriteLine("=== Arguments ===");
        _output.WriteLine("user.name: Jane, user.isPremium: false");
        _output.WriteLine("orders: [X1: Phone (800€), Y2: Case (20€)]");
        _output.WriteLine("=== Rendered Output ===");
        _output.WriteLine(rendered);

        rendered.ShouldContain("Hello Jane!");
        rendered.ShouldContain("Consider upgrading");
        rendered.ShouldContain("Phone");
        rendered.ShouldContain("Case");
    }

    [Fact]
    public async Task HandlebarsPromptExecutor_HandlesMissingCollection()
    {
        var executor = new Handlebars.PromptExecutor(_mockChatClient.Object);

        var template = """
            Orders:
            {{#each orders}}
            - {{id}}
            {{/each}}
            """;

        var arguments = new KernelArguments(); // orders missing

        var kernel = GetKernel(executor);
        var factory = new HandlebarsPromptTemplateFactory();
        var templateObj = factory.Create(new PromptTemplateConfig
        {
            Name = "HandlebarsMissingCollection",
            Template = template,
            TemplateFormat = "handlebars",
            AllowDangerouslySetContent = true
        });

        var rendered = await templateObj.RenderAsync(kernel, arguments);

        _output.WriteLine("=== Handlebars Template Input (Missing Collection) ===");
        _output.WriteLine(template);
        _output.WriteLine("=== Arguments ===");
        _output.WriteLine("orders: missing");
        _output.WriteLine("=== Rendered Output ===");
        _output.WriteLine(rendered);

        rendered.ShouldContain("Orders:");
        rendered.ShouldNotContain("-"); // No items should be rendered
    }

    [Fact]
    public async Task HandlebarsPromptExecutor_HandlesEmptyCollection()
    {
        var executor = new Handlebars.PromptExecutor(_mockChatClient.Object);

        var template = """
            Orders:
            {{#each orders}}
            - {{id}}
            {{/each}}
            """;

        var arguments = new KernelArguments
        {
            ["orders"] = new Dictionary<string, object>[0] // empty array
        };

        var kernel = GetKernel(executor);
        var factory = new HandlebarsPromptTemplateFactory();
        var templateObj = factory.Create(new PromptTemplateConfig
        {
            Name = "HandlebarsEmptyCollection",
            Template = template,
            TemplateFormat = "handlebars",
            AllowDangerouslySetContent = true
        });

        var rendered = await templateObj.RenderAsync(kernel, arguments);

        _output.WriteLine("=== Handlebars Template Input (Empty Collection) ===");
        _output.WriteLine(template);
        _output.WriteLine("=== Arguments ===");
        _output.WriteLine("orders: [] (empty array)");
        _output.WriteLine("=== Rendered Output ===");
        _output.WriteLine(rendered);

        rendered.ShouldContain("Orders:");
        rendered.ShouldNotContain("-"); // No items should be rendered
    }

    // -------------------------
    // EXECUTION TESTS
    // -------------------------

    [Fact]
    public async Task PromptExecutor_ExecuteAsync_WithComplexLiquidTemplate()
    {
        var executors = new List<IPromptExecutor>
        {
            new Liquid.PromptExecutor(_mockChatClient.Object)
        };

        var promptExecutor = new PromptExecutor(executors);

        var template = "Hello {{name}}";
        var arguments = new KernelArguments
        {
            ["name"] = "World"
        };

        _output.WriteLine("=== Liquid Execution Test ===");
        _output.WriteLine($"Template: {template}");
        _output.WriteLine("Arguments: name = World");
        _output.WriteLine("Expected Rendered: Hello World");

        var result = await promptExecutor.ExecuteAsync(
            "liquid",
            template,
            arguments);

        _output.WriteLine($"Execution Result: {result}");

        result.ShouldBe("Mock response");
    }

    [Fact]
    public async Task HandlebarsPromptExecutor_ExecuteStreamingAsync_WithComplexTemplate()
    {
        var executor = new Handlebars.PromptExecutor(_mockChatClient.Object);

        var template = "Hello {{name}}";

        var arguments = new KernelArguments
        {
            ["name"] = "World"
        };

        _output.WriteLine("=== Handlebars Streaming Execution Test ===");
        _output.WriteLine($"Template: {template}");
        _output.WriteLine("Arguments: name = World");
        _output.WriteLine("Expected Rendered: Hello World");

        var stream = executor.ExecuteStreamingAsync(template, arguments);
        var results = await stream.ToListAsync();

        _output.WriteLine($"Streaming Results Count: {results.Count}");
        for (int i = 0; i < results.Count; i++)
        {
            _output.WriteLine($"Result {i}: {results[i]}");
        }

        results.ShouldNotBeEmpty();
    }

    // -------------------------
    // Helpers
    // -------------------------

    private static Kernel GetKernel(object executor)
    {
        return executor
            .GetType()
            .GetField("_kernel",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)!
            .GetValue(executor) as Kernel
            ?? throw new InvalidOperationException("Kernel not found");
    }
}