using Microsoft.Extensions.AI;

namespace AIKit.Core.Clients;

public interface IChatClientProvider
{
    string Provider { get; }
    IChatClient Create(string? modelName = null);
    IChatClient Create(AIClientSettings settings, string? modelName = null);
}
