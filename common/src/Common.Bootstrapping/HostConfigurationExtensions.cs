using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Hosting;

namespace Common.Bootstrapping;

public static class HostConfigurationExtensions
{
    public static IHostApplicationBuilder AddConfiguration(
        this IHostApplicationBuilder builder)
    {
        builder.Configuration.AddAzureAppConfiguration(options =>
        {
            // Use the App Configuration endpoint
       //     string endpoint = builder.Configuration["AppConfigEndpoint"];

            options.Connect(
                    new Uri("https://ac-dev-eastus-o4wtmt.azconfig.io"),
                    new DefaultAzureCredential())
                // 1. Load keys with NO label first (default values)
                // .Select(KeyFilter.Any, LabelFilter.Null)
                // 2. Load keys with a SPECIFIC label (e.g., "Development") 
                // These will override any matching keys from the first Select call
                .Select(KeyFilter.Any, "demo-one-api"); 
        });
        
        return builder;
    }
    
}