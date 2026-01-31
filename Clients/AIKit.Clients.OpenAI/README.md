# AIKit.Clients.OpenAI

[![NuGet Version](https://img.shields.io/nuget/v/AIKit.Clients.OpenAI.svg)](https://www.nuget.org/packages/AIKit.Clients.OpenAI/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/AIKit.Clients.OpenAI.svg)](https://www.nuget.org/packages/AIKit.Clients.OpenAI/)
[![License: Apache-2.0](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

## Description

OpenAI Chat, Embeddings

## Installation

```bash
dotnet add package AIKit.Clients.OpenAI --version 1.0.0
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

For detailed API documentation, see [API Reference](https://github.com/durmisi/AIKit/docs/api/AIKit.Clients.OpenAI).

## Links

- [GitHub Repository](https://github.com/durmisi/AIKit)
- [Issues](https://github.com/durmisi/AIKit/issues)
- [License](https://github.com/durmisi/AIKit/blob/main/LICENSE)
