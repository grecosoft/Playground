using System.Collections.Concurrent;
using Messaging.Hub.Domain;

namespace Messaging.Hub.Infra;

public class ConnectionManager : IConnectionManager
{
    private readonly ConcurrentDictionary<string, string> _connections = new();

    public void AddConnection(string agentId, string connectionId)
    {
        _connections.TryAdd(agentId, connectionId);
    }

    public void RemoveConnection(string agentId)
    {
        _connections.TryRemove(agentId, out _);
    }

    public string? GetConnection(string agentId)
    {
        return _connections.GetValueOrDefault(agentId);
    }
}