using Common.Bootstrapping;
using Common.Messaging;
using Common.Messaging.Commands;
using Common.Messaging.Core;
using Common.Messaging.Core.Commands;
using Messaging.Demo.Common;
using Messaging.Demo.Common.Commands;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure the Token Credential be registered within the container and load configuration:
var credential = builder.AddTokenCredential();
builder.AddConfiguration(credential);

var boostrapLogger = builder.AddLogging(logConfig =>
{
    logConfig.AddServiceProperties(builder.Configuration);
    
    logConfig.WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
     
    var seqUrl = builder.Configuration.GetValue<string>("Logging:SeqUrl") ?? string.Empty;
    if (!string.IsNullOrWhiteSpace(seqUrl))
    {
        logConfig.WriteTo.Seq(seqUrl);
    }
});

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
    // app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI();

// app.UseHttpsRedirection();


app.MapPost("/send-rpc-command", async (
    ICommandMessaging commandMessaging,
    CancellationToken cancellationToken) =>
{
    var command = new PingCommand("value-one", "value-two");
    var endPoint = commandMessaging.GetServiceEndpoint(ServiceEndpoints.DemoServiceTwo);
    
    var response = await endPoint.SendCommandWithReplyAsync(command, cancellationToken);
    return Results.Ok(response);
});

app.MapPost("/send-async-command", async (
    ICommandMessaging commandMessaging,
    CancellationToken cancellationToken) =>
{
    var command = new DeviceUpdateCommand(Guid.NewGuid().ToString());
    var endPoint = commandMessaging.GetServiceEndpoint(ServiceEndpoints.DemoServiceTwo);
    
    await endPoint.SendCommandAsync(command, cancellationToken);
    return Results.Ok();
});

app.MapPost("commands/{connectorId}/ping", async (
    string  connectorId,
    string message,
    ICommandMessaging commandMessaging,
    CancellationToken ct) =>
{
    var command = new ConnectorPingCommand(connectorId, message);
    var endPoint = commandMessaging.GetServiceEndpoint(ServiceEndpoints.MessagingHubApi);

    try
    {
        var response = await endPoint.SendCommandWithReplyAsync(command, ct, new CommandOptions { ThrowIfErrorResponse = true });
        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        return  Results.BadRequest(ex.Message);
    }
    
});

app.MapPost("commands/{connectorId}/status", async (
    string  connectorId,
    string minLogSeverity,
    ICommandMessaging commandMessaging,
    CancellationToken ct) =>
{
    var command = new ConnectorStatusCommand(connectorId, minLogSeverity);
    var endPoint = commandMessaging.GetServiceEndpoint(ServiceEndpoints.MessagingHubApi);
    
    await endPoint.SendCommandAsync(command, ct);
    return Results.Ok();
});

app.Run();
