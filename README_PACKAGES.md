# AIKit Client Packages

AIKit provides modular NuGet packages for various AI providers. Install only the packages you need.

## Core Package

Install the core package first:

```
dotnet add package AIKit.Core.Clients --version 1.0.0
```

## Provider Packages

Choose and install the desired provider(s):

- **OpenAI**: `dotnet add package AIKit.Clients.OpenAI --version 1.0.0`
- **Azure OpenAI**: `dotnet add package AIKit.Clients.AzureOpenAI --version 1.0.0`
- **Ollama**: `dotnet add package AIKit.Clients.Ollama --version 1.0.0`
- **Gemini**: `dotnet add package AIKit.Clients.Gemini --version 1.0.0`
- **AWS Bedrock**: `dotnet add package AIKit.Clients.Bedrock --version 1.0.0`
- **GitHub Models**: `dotnet add package AIKit.Clients.GitHub --version 1.0.0`
- **Azure Claude**: `dotnet add package AIKit.Clients.AzureClaude --version 1.0.0`
- **Claude** (Anthropic): `dotnet add package AIKit.Clients.Claude --version 1.0.0`

## Usage Example

```csharp
using AIKit.Core.Clients;

// Configure settings
var settings = new Dictionary<string, object>
{
    ["ApiKey"] = "your-key",
    ["ModelId"] = "gpt-4o"
};

// Create provider
IChatClientProvider provider = new OpenAIChatClientProvider(settings);

// Get client
IChatClient client = provider.Create();
```

## Publishing

To publish packages to NuGet.org:

1. Obtain an API key from https://www.nuget.org/account/apikeys
2. For each package: `dotnet nuget push <path-to-nupkg> --api-key <your-key> --source https://api.nuget.org/v3/index.json`

For private feeds, adjust the source URL accordingly.

---

## Deprecated types ⚠️

- **`BaseEmbeddingGeneratorFactory`**: Deprecated — this base class will be removed in a future release. Implement `IEmbeddingGeneratorFactory` directly in your provider factory and move constructor validation, `Provider`, `Create()`/`Create(settings)`, and any `RetryPolicy` handling into the concrete implementation.
</content>
<parameter name="filePath">README_PACKAGES.md
