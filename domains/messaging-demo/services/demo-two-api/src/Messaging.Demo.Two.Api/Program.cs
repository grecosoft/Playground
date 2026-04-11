using Common.Bootstrapping;
using Common.Messaging.Commands;
using Common.Messaging.Core;
using Common.Messaging.Core.Commands;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

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

// app.MapGet("/send-async-response/{correlationId}", async (
//     string correlationId, 
//     ICommandRepository commandRepository,
//     ICommandMessagingService  messagingService,
//     CancellationToken cancellationToken) =>
// {
//     var receivedCommand = await commandRepository.LoadCommand<DeviceUpdateCommand>(correlationId, cancellationToken);
//     var command = (DeviceUpdateCommand)receivedCommand.Command;
//     
//     command.Response = new DeviceStatus(true);
//     
//     await messagingService.SendResponseToCommandAsync(
//         receivedCommand, 
//         command,
//         cancellationToken);
//     
//     return Results.Ok();
// });



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.Run();


