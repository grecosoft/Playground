using Common.Bootstrapping;
using Messaging.Hub.Infra;
using Microsoft.Azure.SignalR.Management;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var credential = builder.AddTokenCredential();
builder.AddConfiguration(credential);

var boostrapLogger = builder.AddLogging(c =>
{
    c.WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
    
    c.WriteTo.Seq(builder.Configuration["Logging:SeqUrl"] ?? "http://localhost:5341");
});

builder.AddSignalRMessaging(boostrapLogger);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();


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

app.MapGet("/hub/token", async (
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
