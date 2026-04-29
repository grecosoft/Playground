using Common.Messaging.Commands;
using Messaging.Demo.Common.Commands;
using Messaging.Hub.Api.Services;
using Messaging.Hub.Domain;
using Microsoft.AspNetCore.SignalR;

namespace Messaging.Hub.Api.Handlers;

public class AgentPingHandler(
    IHubContext<ConnectorHub> connectorHub,
    IConnectionManager connectionManager,
    ICommandValidationService validationService)
    : CommandHandlerBase<AgentPingCommand, AgentPingResponse>
{
    protected override async Task HandleMessage(
        AgentPingCommand command,
        CommandContext context,
        CancellationToken ct)
    {
        var agentConnectionId = connectionManager.GetConnection(command.AgentId);
       
        // This is an RPC style command, so we need to wait for the response from
        // the client before we can send a response back to the originating service.  
        var hubCommands = new ConnectorHubCommands(connectorHub, context);
        
        var response = await hubCommands.SendCommandWaitResponse(agentConnectionId!, command, validationService, ct);
        if (response is not null)
        {
            SetResponse(context, response);
        }
    }
}