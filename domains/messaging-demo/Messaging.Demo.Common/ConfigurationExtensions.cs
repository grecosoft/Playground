using Microsoft.Extensions.Configuration;
using Serilog;

namespace Messaging.Demo.Common;

public static class BootstrapExtensions
{
    public static void AddServiceProperties(
        this LoggerConfiguration logConfig, 
        IConfigurationRoot configuration)
    {
        logConfig
            .Enrich.WithProperty("Service", configuration["ServiceName"])
            .Enrich.WithProperty("Service", configuration["SolutionEnvironment"])
            .Enrich.WithProperty("Host", Environment.MachineName);
    }
}