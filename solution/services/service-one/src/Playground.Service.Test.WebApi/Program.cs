using Playground.Common;
using Playground.Common.Messaging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddBusMessaging(builder.Configuration, "solution");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/send-command", async (
    IMessagingService  microserviceTwo,
    CancellationToken cancellationToken) =>
{

    var command = new PingCommand("value-one", "value-two");
    var response = await microserviceTwo.SendCommandWithReplyAsync(command, cancellationToken);
    return Results.Ok(response);
    
});

app.MapGet("/send-command2", async (
    IMessagingService  microserviceTwo,
    CancellationToken cancellationToken) =>
{

    var command = new DeviceUpdate(Guid.NewGuid().ToString());
    await microserviceTwo.SendCommandWithResponseAsync(command, cancellationToken);
    return Results.Ok();
    
});

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
