# csharp-connector

This is a simple API representing a service participating in receiving cloud commands sent from services and communicating responses back to the cloud.

While all backend services communicate sending commands over Azure Service Bus, this service communicates with the backend services by receiving and replaying to commands using SignalR.  The messaging-hub-api service is responsible for receiving commands from Azure Service Bus and sending them to services such as **acme-agant-api** using SignalR. This service is responsible for receiving the command, processing it, and sending a response back to the messaging-hub-api service using SignalR, which then forwards the response back to the originating service over Azure Service Bus.