using Common.Bootstrapping;
using Common.Messaging.Commands;
using Common.Messaging.Core;
using Common.Messaging.Core.Commands;
using Messaging.Demo.Common;
using Messaging.Demo.Two.Api.Models;
using Messaging.Hub.Api;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

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

app.MapConnectorHub();

app.MapGet("/pending/commands", async(ICommandRepository repository) =>
{
    var commands = await repository.GetPendingCommandContextsAsync(ct: CancellationToken.None);
    return Results.Ok(commands.Select(c => new PendingCommandModel(c)));
});

app.Run();
