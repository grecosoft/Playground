using Playground.Common;
using Playground.Common.Messaging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddBusMessaging(builder.Configuration, "solution", "solution-service-one-commands");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/send-command", async (
    [FromKeyedServices("service-two")]IServiceEndpoint  microserviceTwo,
    CancellationToken cancellationToken) =>
{
    var command = new PingCommand("value-one", "value-two");
    var response = await microserviceTwo.SendCommandWithReplyAsync(command, cancellationToken);
    return Results.Ok(response);
});

app.MapGet("/send-command2", async (
    [FromKeyedServices("service-two")]IServiceEndpoint  microserviceTwo,
    CancellationToken cancellationToken) =>
{

    var command = new DeviceUpdate(Guid.NewGuid().ToString());
    await microserviceTwo.SendCommandAsync(command, cancellationToken);
    return Results.Ok();
});

app.Run();

