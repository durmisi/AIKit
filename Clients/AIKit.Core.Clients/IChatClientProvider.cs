using Microsoft.Extensions.AI;

namespace AIKit.Core.Clients;

public interface IChatClientProvider
{
    string Provider { get; }
    IChatClient Create();
    IChatClient Create(AIClientSettings settings);
}
