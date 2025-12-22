using AIKit.Core.VectorStores;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.AI;

namespace AIKit.Core.Vector;

/// <summary>
/// Reusable helper methods for vector store providers.
/// Contains common patterns for credential resolution, client configuration, and validation.
/// </summary>
public static class VectorStoreProviderHelpers
{
    /// <summary>
    /// Resolves Azure credentials from settings. Supports ClientSecretCredential and DefaultAzureCredential.
    /// Can be used in Azure-based providers (CosmosNoSQL, CosmosMongoDB, AzureAISearch).
    /// </summary>
    public static TokenCredential ResolveAzureCredential(VectorStoreSettings settings)
    {
        if (!string.IsNullOrWhiteSpace(settings.ClientId) && !string.IsNullOrWhiteSpace(settings.ClientSecret))
        {
            return new ClientSecretCredential(
                settings.TenantId ?? throw new InvalidOperationException("TenantId must be provided when using ClientId and ClientSecret."),
                settings.ClientId,
                settings.ClientSecret);
        }

        return new DefaultAzureCredential();
    }

    /// <summary>
    /// Ensures an embedding generator is provided. Throws if null.
    /// Can be used in all vector store providers that require embeddings.
    /// </summary>
    public static IEmbeddingGenerator ResolveEmbeddingGenerator(VectorStoreSettings settings)
    {
        return settings.EmbeddingGenerator ?? throw new InvalidOperationException(
            "An IEmbeddingGenerator must be provided in VectorStoreSettings.");
    }

    /// <summary>
    /// Resolves JSON serializer options with smart defaults (indented in debug mode).
    /// Can be used in providers that support JSON serialization options.
    /// </summary>
    public static System.Text.Json.JsonSerializerOptions ResolveJsonSerializerOptions(VectorStoreSettings settings)
    {
        return settings.JsonSerializerOptions ?? new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = System.Diagnostics.Debugger.IsAttached,
        };
    }

    /// <summary>
    /// Gets a typed value from AdditionalSettings dictionary with type checking.
    /// Returns default(T) if key not found or wrong type.
    /// </summary>
    public static T? GetAdditionalSetting<T>(VectorStoreSettings settings, string key)
    {
        if (settings.AdditionalSettings?.TryGetValue(key, out var value) == true && value is T typedValue)
        {
            return typedValue;
        }
        return default;
    }

    /// <summary>
    /// Gets a typed value from AdditionalSettings dictionary with type checking.
    /// Returns provided defaultValue if key not found or wrong type.
    /// </summary>
    public static T GetAdditionalSetting<T>(VectorStoreSettings settings, string key, T defaultValue)
    {
        if (settings.AdditionalSettings?.TryGetValue(key, out var value) == true && value is T typedValue)
        {
            return typedValue;
        }
        return defaultValue;
    }
}