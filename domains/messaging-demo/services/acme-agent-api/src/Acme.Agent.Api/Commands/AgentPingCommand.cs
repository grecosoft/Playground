namespace Acme.Agent.Api.Commands;

public record AgentPingCommand(
    Guid CompanyId,
    string AgentId,
    string EchoMessage) ;