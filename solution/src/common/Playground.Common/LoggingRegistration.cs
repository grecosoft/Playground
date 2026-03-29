using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Playground.Common;

public static class LoggingRegistration
{
    public static ILogger AddLogging(this IHostApplicationBuilder _)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties}{NewLine}{Exception}")
            .CreateLogger()
            .ForContext("root", "Playground.Common");

        return new SerilogLoggerFactory(Log.Logger)
            .CreateLogger("bootstrap");
    }
}