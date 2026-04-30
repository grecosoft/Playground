using System.Security.Claims;
using Azure.Identity;
using Messaging.Hub.Api.Services;
using Messaging.Hub.Domain;
using Messaging.Hub.Infra;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.Azure.SignalR;

namespace Messaging.Hub.Api;

public static class ServiceRegistrations
{
    public static IHostApplicationBuilder AddSignalRMessaging(
        this IHostApplicationBuilder builder,
        ILogger bootstrapLogger)
    {
        var config = builder.Configuration.Get<ConnectorHubConfig>() 
                     ?? throw new NullReferenceException("MessagingHubConfig is null");
        
        bootstrapLogger.LogInformation(
            "Configuring Service Messaging: {@Configuration}", config.ToLoggableProperties());
        
        var credential = new DefaultAzureCredential();
        
        builder.Services.AddSignalR().AddAzureSignalR(options =>
        {
            options.ServerStickyMode = ServerStickyMode.Required;
       
            options.Endpoints =
            [
                new ServiceEndpoint(new Uri(config.SignalREndpoint), credential)
            ];
            
            options.ClaimsProvider = context =>
            {
                // NOTE:  This is just for the POC.  The first step is that the client needs to
                // Call an api of the service to obtain a JTW containing these claims.
                var agentIdentity = context.Request.Query["agentIdentity"].ToString();
                return
                [
                    new Claim(ClaimTypes.NameIdentifier, agentIdentity)
                ];
            };
        });
        
        builder.Services.AddSingleton<IAgentRepository, AgentRepository>();
        builder.Services.AddSingleton<IConnectorHubManager, ConnectorHubManager>();
        
        AddSchemaValidation(bootstrapLogger, builder, config);
        return builder;
    }
    
    private static void AddSchemaValidation(
        ILogger bootstrapLogger,
        IHostApplicationBuilder builder,
        ConnectorHubConfig config)
    {
        var commandSchemaPath = Path.Join(AppContext.BaseDirectory, config.CommandSchemaRootPath);
        var commandValidation = new CommandValidationService(commandSchemaPath);        
        
        commandValidation.LoadSchemas(bootstrapLogger,[
            new CommandSchema("agent.commands.ping", "AgentPing"),
            new CommandSchema("agent.commands.status", "AgentStatusSummary")
        ]);
        
        builder.Services.AddSingleton<ICommandValidationService>(commandValidation);
    }
    
    public static IApplicationBuilder MapConnectorHub(this IApplicationBuilder appBuilder)
    {
        appBuilder.UseWebSockets();
        appBuilder.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<ConnectorHub>("/connectorhub", options =>
            {
                options.Transports = HttpTransportType.WebSockets;
            });
        });
        
        return appBuilder;
    }
}
