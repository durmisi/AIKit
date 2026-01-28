using AIKit.Clients.Interfaces;
using AIKit.Clients.Resilience;
using AIKit.Clients.Settings;
using Microsoft.Extensions.AI;

namespace AIKit.Clients.Base;

/// <summary>
/// Base class for embedding generator factories, providing common functionality.
/// </summary>
public abstract class BaseEmbeddingGeneratorFactory : IEmbeddingGeneratorFactory
{
    /// <summary>
    /// The default AI client settings.
    /// </summary>
    protected readonly AIClientSettings _defaultSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseEmbeddingGeneratorFactory"/> class.
    /// </summary>
    /// <param name="settings">The AI client settings.</param>
    protected BaseEmbeddingGeneratorFactory(AIClientSettings settings)
    {
        _defaultSettings = settings ?? throw new ArgumentNullException(nameof(settings));
        Validate(_defaultSettings);
    }

    /// <summary>
    /// Gets the provider name.
    /// </summary>
    public string Provider => _defaultSettings.ProviderName ?? GetDefaultProviderName();

    /// <summary>
    /// Creates an embedding generator using the default settings.
    /// </summary>
    /// <returns>The created embedding generator.</returns>
    public IEmbeddingGenerator<string, Embedding<float>> Create()
        => Create(_defaultSettings);

    /// <summary>
    /// Creates an embedding generator with the specified settings.
    /// </summary>
    /// <param name="settings">The AI client settings.</param>
    /// <returns>The created embedding generator.</returns>
    public IEmbeddingGenerator<string, Embedding<float>> Create(AIClientSettings settings)
    {
        Validate(settings);

        var generator = CreateGenerator(settings);

        if (settings.RetryPolicy != null)
        {
            return new RetryEmbeddingGenerator(generator, settings.RetryPolicy);
        }

        return generator;
    }

    /// <summary>
    /// Gets the default provider name if not specified in settings.
    /// </summary>
    protected abstract string GetDefaultProviderName();

    /// <summary>
    /// Validates the settings for this provider.
    /// </summary>
    protected abstract void Validate(AIClientSettings settings);

    /// <summary>
    /// Creates the actual embedding generator instance.
    /// </summary>
    protected abstract IEmbeddingGenerator<string, Embedding<float>> CreateGenerator(AIClientSettings settings);
}