using Acme.Agent.Api;
using Common.Bootstrapping;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure the Token Credential be registered within the container and load configuration:
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


app.MapGet("/send-command-response", () => Results.Ok());

app.Run();
