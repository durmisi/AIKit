using AIKit.Clients.Settings;
using Microsoft.Extensions.AI;

namespace AIKit.Clients.Interfaces;

/// <summary>
/// Provides embedding generators for AI interactions.
/// Implementations create instances of <see cref="IEmbeddingGenerator{TInput, TEmbedding}"/> for specific AI providers.
/// </summary>
public interface IEmbeddingGeneratorFactory
{
    /// <summary>
    /// Gets the name of the provider (e.g., "open-ai", "azure-open-ai").
    /// Used for identification and registration.
    /// </summary>
    string Provider { get; }

    /// <summary>
    /// Creates an embedding generator using the default settings.
    /// </summary>
    /// <returns>An <see cref="IEmbeddingGenerator{TInput, TEmbedding}"/> instance.</returns>
    IEmbeddingGenerator<string, Embedding<float>> Create();

    /// <summary>
    /// Creates an embedding generator with custom settings.
    /// </summary>
    /// <param name="settings">The settings to use for the generator.</param>
    /// <returns>An <see cref="IEmbeddingGenerator{TInput, TEmbedding}"/> instance.</returns>
    IEmbeddingGenerator<string, Embedding<float>> Create(AIClientSettings settings);
}