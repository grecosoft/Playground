using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

namespace Common.Bootstrapping;

public static class HostLoggingExtensions
{
    public static Microsoft.Extensions.Logging.ILogger AddLogging(
        this IHostApplicationBuilder builder,
        Action<LoggerConfiguration> configure)
    {
        var loggerConfiguration = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext();
        
        configure(loggerConfiguration);
        
        Log.Logger = loggerConfiguration.CreateLogger();
        
        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(Log.Logger);
        
        return new SerilogLoggerFactory(Log.Logger)
            .CreateLogger("bootstrap");
    }
}