using Microsoft.Extensions.AI;
using System.Diagnostics;

namespace AIKit.Clients;

/// <summary>
/// A chat client that wraps another client with retry logic.
/// </summary>
public sealed class RetryChatClient : IChatClient
{
    private readonly IChatClient _innerClient;
    private readonly RetryPolicySettings _retrySettings;

    public RetryChatClient(IChatClient innerClient, RetryPolicySettings retrySettings)
    {
        _innerClient = innerClient ?? throw new ArgumentNullException(nameof(innerClient));
        _retrySettings = retrySettings ?? throw new ArgumentNullException(nameof(retrySettings));
    }

    public async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        var exceptions = new List<Exception>();
        var delay = TimeSpan.FromSeconds(_retrySettings.InitialDelaySeconds);

        for (int attempt = 0; attempt <= _retrySettings.MaxRetries; attempt++)
        {
            try
            {
                return await _innerClient.GetResponseAsync(messages, options, cancellationToken);
            }
            catch (Exception ex) when (IsTransientException(ex))
            {
                exceptions.Add(ex);
                if (attempt < _retrySettings.MaxRetries)
                {
                    await Task.Delay(delay, cancellationToken);
                    delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * _retrySettings.BackoffMultiplier, _retrySettings.MaxDelaySeconds));
                }
            }
        }

        throw new AggregateException($"Failed after {_retrySettings.MaxRetries + 1} attempts. See inner exceptions for details.", exceptions);
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        await foreach (var response in _innerClient.GetStreamingResponseAsync(messages, options, cancellationToken))
        {
            yield return response;
        }
    }

    private static bool IsTransientException(Exception ex)
    {
        // Consider network, timeout, rate limit, and server errors as transient
        return ex is HttpRequestException ||
               ex is TimeoutException ||
               (ex is InvalidOperationException && ex.Message.Contains("rate limit")) ||
               (ex is InvalidOperationException && ex.Message.Contains("429")) ||
               (ex is InvalidOperationException && ex.Message.Contains("502")) ||
               (ex is InvalidOperationException && ex.Message.Contains("503")) ||
               (ex is InvalidOperationException && ex.Message.Contains("504"));
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => _innerClient.GetService(serviceType, serviceKey);

    public void Dispose() => _innerClient.Dispose();
}