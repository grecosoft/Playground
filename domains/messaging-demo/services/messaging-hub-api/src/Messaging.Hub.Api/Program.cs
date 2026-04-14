using Common.Bootstrapping;
using Common.Messaging.Commands;
using Common.Messaging.Core;
using Common.Messaging.Core.Commands;
using Messaging.Hub.Infra;
using Microsoft.Azure.SignalR.Management;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var credential = builder.AddTokenCredential();
builder.AddConfiguration(credential);

var boostrapLogger = builder.AddLogging(logConfig =>
{
    logConfig.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
     
    var seqUrl = builder.Configuration.GetValue<string>("Logging:SeqUrl") ?? string.Empty;
    if (!string.IsNullOrWhiteSpace(seqUrl))
    {
        logConfig.WriteTo.Seq(seqUrl);
    }
});

builder.AddSignalRMessaging(boostrapLogger);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

builder.Services.AddBusMessaging(
    boostrapLogger,
    builder.Configuration);

builder.Services.AddSingleton<ICommandRepository, CommandRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseRouting();

app.MapGet("/hub/{agentId}/token", async (
    string agentId,
    ServiceManager manager,
    CancellationToken cancellationToken) =>
{
    var serviceHubContext = await manager.CreateHubContextAsync("ConnectorHub/negotiate", cancellationToken);
    var negotiationResponse = await serviceHubContext.NegotiateAsync(new NegotiationOptions
    {
        // TODO:  Set userid and other claims if needed.
        // See https://learn.microsoft.com/azure/azure-signalr/signalr-concept-negotiation?tabs=serverless%2Csignal
    }, cancellationToken);
    return Results.Ok(negotiationResponse.AccessToken);
});

app.MapConnectorHub();
app.Run();
