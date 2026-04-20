namespace Messaging.Hub.Domain;

public interface IConnectionManager
{
    void AddConnection(string agentId, string connectionId);

    void RemoveConnection(string agentId);
    
    string? GetConnection(string agentId);
}