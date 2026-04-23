using System.Text.Json;
using Acme.Agent.Api;
using Acme.Agent.Api.Commands;
using Common.Bootstrapping;
using Microsoft.AspNetCore.SignalR.Client;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure the Token Credential be registered within the container and load configuration:
var credential = builder.AddTokenCredential();
builder.AddConfiguration(credential);

var boostrapLogger = builder.AddLogging(logConfig =>
{
    logConfig
        .Enrich.WithProperty("Service", builder.Configuration["ServiceName"])
        .Enrich.WithProperty("Service", builder.Configuration["SolutionEnvironment"])
        .Enrich.WithProperty("Host", Environment.MachineName);
    
     logConfig.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
     
     var seqUrl = builder.Configuration.GetValue<string>("Logging:SeqUrl") ?? string.Empty;
     if (!string.IsNullOrWhiteSpace(seqUrl))
     {
         logConfig.WriteTo.Seq(seqUrl);
     }
});

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.AddServices(boostrapLogger);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI();

// app.UseHttpsRedirection();


app.MapPost("/send-status-summary/{correlationId}/", async (string correlationId, HubConnection hub) =>
{
    var response = new AgentStatusSummaryResponse(
        DateTime.UtcNow,
        LogSeverityType.Warning,
        [
            new ComponentStatus("typewriter", DateTime.UtcNow, "Disk space low", LogSeverityType.Warning),
            new ComponentStatus("coffee-machine", DateTime.UtcNow, "Out of coffee", LogSeverityType.Error)
        ]);
    
    await hub.SendAsync(
        "SendResponseToCommand",
        correlationId, 
        JsonSerializer.Serialize(response));
});

app.Run();
