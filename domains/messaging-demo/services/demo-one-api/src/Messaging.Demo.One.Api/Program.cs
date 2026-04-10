using Common.Bootstrapping;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var credential = builder.AddTokenCredential();
var boostrapLogger = builder.AddLogging(c =>
{
    c.WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
});

builder.AddConfiguration(boostrapLogger, credential);

// builder.Services.AddBusMessaging(
//     boostrapLogger,
//     builder.Configuration);
//
// builder.Services.AddSingleton<ICommandRepository, CommandRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


// app.MapGet("/send-rpc-command", async (
//     [FromKeyedServices("demo-two-api")]ICommandEndpoint  microserviceTwo,
//     CancellationToken cancellationToken) =>
// {
//     var command = new PingCommand("value-one", "value-two");
//     var response = await microserviceTwo.SendCommandWithReplyAsync(command, cancellationToken);
//     return Results.Ok(response);
// });
//
// app.MapGet("/send-async-command", async (
//     [FromKeyedServices("demo-two-api")]ICommandEndpoint  microserviceTwo,
//     CancellationToken cancellationToken) =>
// {
//
//     var command = new DeviceUpdateCommand(Guid.NewGuid().ToString());
//     await microserviceTwo.SendCommandAsync(command, cancellationToken);
//     return Results.Ok();
// });

app.Run();
