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
    /// The default settings.
    /// </summary>
    protected readonly Dictionary<string, object> _defaultSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseEmbeddingGeneratorFactory"/> class.
    /// </summary>
    /// <param name="settings">The settings as key-value pairs.</param>
    protected BaseEmbeddingGeneratorFactory(Dictionary<string, object> settings)
    {
        _defaultSettings = settings ?? throw new ArgumentNullException(nameof(settings));
        Validate(_defaultSettings);
    }

    /// <summary>
    /// Gets the provider name.
    /// </summary>
    public string Provider => GetProviderName(_defaultSettings);

    /// <summary>
    /// Creates an embedding generator using the default settings.
    /// </summary>
    /// <returns>The created embedding generator.</returns>
    public IEmbeddingGenerator<string, Embedding<float>> Create()
        => Create(_defaultSettings);

    /// <summary>
    /// Creates an embedding generator with the specified settings.
    /// </summary>
    /// <param name="settings">The settings as key-value pairs.</param>
    /// <returns>The created embedding generator.</returns>
    public IEmbeddingGenerator<string, Embedding<float>> Create(Dictionary<string, object> settings)
    {
        Validate(settings);

        var generator = CreateGenerator(settings);

        if (settings.TryGetValue("RetryPolicy", out var retryObj) && retryObj is RetryPolicySettings retryPolicy)
        {
            return new RetryEmbeddingGenerator(generator, retryPolicy);
        }

        return generator;
    }

    /// <summary>
    /// Gets the provider name from settings.
    /// </summary>
    protected abstract string GetProviderName(Dictionary<string, object> settings);

    /// <summary>
    /// Validates the settings for this provider.
    /// </summary>
    protected abstract void Validate(Dictionary<string, object> settings);

    /// <summary>
    /// Creates the actual embedding generator instance.
    /// </summary>
    protected abstract IEmbeddingGenerator<string, Embedding<float>> CreateGenerator(Dictionary<string, object> settings);
}