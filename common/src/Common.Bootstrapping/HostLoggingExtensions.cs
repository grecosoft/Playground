using Microsoft.Extensions.Hosting;
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
        
        return new SerilogLoggerFactory(Log.Logger)
            .CreateLogger("bootstrap");
    }
}