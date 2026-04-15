using Common.Bootstrapping;
using Common.Messaging.Commands;
using Common.Messaging.Core;
using Common.Messaging.Core.Commands;
using Messaging.Hub.Api;
using Messaging.Hub.Api.Models;
using Messaging.Hub.Domain;
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

app.MapGet("{customerId:guid}/hub/{identity}/token", async (
    Guid customerId,
    string identity,
    IAgentNegotiateService negotiationService,
    CancellationToken ct) =>
{
    var result = await negotiationService.GetTokenAsync(customerId, identity, ct);
    return result switch
    {
        { AgentNotFound: true } => Results.NotFound(),
        { TokenNotGenerated: true } => Results.BadRequest("Token could not be generated"),
        _ => Results.Ok(new NegotiationModel(result.Token))
    };
});

app.MapConnectorHub();
app.Run();
