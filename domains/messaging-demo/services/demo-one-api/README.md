# demo-one-api
This is a simple internal API showing how commands can be sent to other internal services and external agents.

## Internal Service Communication Examples

- send-rpc-command: Sends RPC style command to the demo-two-api service and expects a response.
- send-async-command: Sends a command to the demo-two-api service without waiting for a response.  The demo-two-api service will process the command and send a response back to the demo-one-api service when it's done.

## External Service Communication Examples
- /company/{companyId:guid}/{agentId}/ping: Sends a ping command to an agent via the messaging hub.  The messaging hub forwards the command to the agent in real time using SignalR, and waits for a response.  The response is returned to the caller as the result of the HTTP request.
- /company/{companyId:guid}/{agentId}/status: Sends a status request command to an agent via the messaging hub.  The messaging hub forwards the command to the agent in real time using SignalR, but does not wait for a response.  The agent processes the command and sends a response back to the messaging hub when it's done, which then forwards the response back to the demo-one-api service over Azure Service Bus.  The demo-one-api service logs the response when it receives it.