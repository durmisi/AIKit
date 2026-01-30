# AIKit.Core.Clients

[![NuGet Version](https://img.shields.io/nuget/v/AIKit.Core.Clients.svg)](https://www.nuget.org/packages/AIKit.Core.Clients/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/AIKit.Core.Clients.svg)](https://www.nuget.org/packages/AIKit.Core.Clients/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Description

Core interfaces and settings for AI client providers (IChatClient, IEmbeddingGenerator, etc.)

## Installation

```bash
dotnet add package AIKit.Core.Clients --version 1.0.0
```

## Usage Example

```csharp
using AIKit.Clients.OpenAI;

// Create a chat client
var chatClient = new ChatClientBuilder()
    .WithApiKey("your-openai-api-key")
    .WithModel("gpt-4")
    .Build();

// Get a response
var response = await chatClient.GetResponseAsync("Explain AIKit in simple terms.");
Console.WriteLine(response.Text);
```

## API Reference

For detailed API documentation, see [API Reference](https://github.com/durmisi/AIKit/docs/api/AIKit.Core.Clients).

## Links

- [GitHub Repository](https://github.com/durmisi/AIKit)
- [Issues](https://github.com/durmisi/AIKit/issues)
- [License](https://github.com/durmisi/AIKit/blob/main/LICENSE)
