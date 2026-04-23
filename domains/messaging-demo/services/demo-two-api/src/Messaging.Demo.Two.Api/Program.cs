using Common.Bootstrapping;
using Common.Messaging;
using Common.Messaging.Commands;
using Common.Messaging.Core;
using Common.Messaging.Core.Commands;
using Messaging.Demo.Common.Commands;
using Messaging.Demo.Two.Api.Models;
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

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

builder.Services.AddBusMessaging(
    boostrapLogger,
    builder.Configuration);

builder.Services.AddSingleton<ICommandRepository, CommandRepository>();


var app = builder.Build();

app.MapGet("/pending/commands", async(ICommandRepository repository) =>
{
    var commands = await repository.GetPendingCommandContextsAsync(ct: CancellationToken.None);
    return Results.Ok(commands.Select(c => new PendingCommandModel(c)));
});

app.MapPost("/send-async-response/{correlationId}", async (
    string correlationId, 
    DeviceStatus deviceStatus,
    ICommandRepository commandRepository,
    ICommandMessaging  messaging,
    CancellationToken cancellationToken) =>
{
    var receivedCommand = await commandRepository.LoadTypedCommandContext<DeviceUpdateCommand>(correlationId, cancellationToken);
    receivedCommand.SetResponse(deviceStatus);
    
    await messaging.SendResponseToCommandAsync(
        receivedCommand, 
        cancellationToken);
    
    return Results.Ok();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.Run();


