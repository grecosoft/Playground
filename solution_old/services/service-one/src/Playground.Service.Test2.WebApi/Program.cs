using Playground.Common;
using Playground.Common.Messaging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddBusMessaging(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseHttpsRedirection();

app.MapGet("/send-response/{correlationId}", async (
    string correlationId, 
    ICommandRepository commandRepository,
    IMessagingService  messagingService,
    CancellationToken cancellationToken) =>
{
    var receivedCommand = await commandRepository.LoadCommand<DeviceUpdate>(correlationId, cancellationToken);
    var command = (DeviceUpdate)receivedCommand.Command;
    
    command.Response = new DeviceStatus(true);
    
    await messagingService.SendResponseToCommandAsync(
        receivedCommand, 
        command,
        cancellationToken);
    
    return Results.Ok();
});



app.Run();
