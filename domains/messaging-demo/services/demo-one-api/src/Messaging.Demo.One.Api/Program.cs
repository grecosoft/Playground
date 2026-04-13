using Common.Bootstrapping;
using Common.Messaging;
using Common.Messaging.Commands;
using Common.Messaging.Core;
using Common.Messaging.Core.Commands;
using Messaging.Demo.Common.Commands;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure the Token Credential be registered within the container and load configuration:
var credential = builder.AddTokenCredential();
builder.AddConfiguration(credential);

var boostrapLogger = builder.AddLogging(c =>
{
    c.WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
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


app.MapGet("/send-rpc-command", async (
    ICommandMessaging commandMessaging,
    CancellationToken cancellationToken) =>
{
    var command = new PingCommand("value-one", "value-two");
    var endPoint = commandMessaging.GetServiceEndpoint("demo-two-api");
    
    var response = await endPoint.SendCommandWithReplyAsync(command, cancellationToken);
    return Results.Ok(response);
});

app.MapGet("/send-async-command", async (
    ICommandMessaging commandMessaging,
    CancellationToken cancellationToken) =>
{
    var command = new DeviceUpdateCommand(Guid.NewGuid().ToString());
    var endPoint = commandMessaging.GetServiceEndpoint("demo-two-api");
    
    await endPoint.SendCommandAsync(command, cancellationToken);
    return Results.Ok();
});

app.Run();
