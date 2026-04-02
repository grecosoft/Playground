using Common.Messaging;
using Common.Messaging.Commands;
using Common.Messaging.Core;
using Common.Messaging.Core.Commands;
using Messaging.Demo.Common.Commands;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddBusMessaging(builder.Configuration);
builder.Services.AddSingleton<ICommandRepository, CommandRepository>();

var app = builder.Build();

app.MapGet("/send-async-response/{correlationId}", async (
    string correlationId, 
    ICommandRepository commandRepository,
    ICommandMessagingService  messagingService,
    CancellationToken cancellationToken) =>
{
    var receivedCommand = await commandRepository.LoadCommand<DeviceUpdateCommand>(correlationId, cancellationToken);
    var command = (DeviceUpdateCommand)receivedCommand.Command;
    
    command.Response = new DeviceStatus(true);
    
    await messagingService.SendResponseToCommandAsync(
        receivedCommand, 
        command,
        cancellationToken);
    
    return Results.Ok();
});



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();


