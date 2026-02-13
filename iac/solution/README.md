# Solution

This IAC refers to the configuration of resources, common among microservices, from which the solution is implemented.
The following are example of such Azure resources:

- App Configuration
- Event Hub
- Service Bus
- Storage Account
- Event Grid

Once these core resources are configured by the Solution IAC, specific microservice IAC configurations define child resources 
associated with the parent solution resources.  The following are examples of child resources:

- Queues/Topics associated with Event Hub
- Microservice specific App Configurations
- Event hubs and consumer groups
- Consumer group check-points saved to storage account 

Microservices adds child resources, to solution defined resources, by referencing the outputs defined within the solution's remote state. 