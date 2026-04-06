using Azure.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Hosting;

namespace Common.Bootstrapping;

public static class HostConfigurationExtensions
{
    public static IHostApplicationBuilder AddConfiguration(
        this IHostApplicationBuilder builder,
        TokenCredential tokenCredential)
    {
        // Add the app settings used when developing locally.   When the service is deployed
        // to Kubernetes, configuration as loaded by the Azure configuration provider and this
        // local file is excluded within the .dockerignore when the image is built.
        builder.Configuration.AddJsonFile("appsettings.local.json", optional: true);
        
        var endpoint = builder.Configuration["Service:AppConfig:Endpoint"];
        if (endpoint is not null)
        {
            builder.Configuration.AddAzureAppConfiguration(options =>
            {
                options.Connect(new Uri(endpoint), tokenCredential);
                var labels = builder.Configuration
                    .GetSection("Service:AppConfig:Labels")
                    .Get<string[]>() ?? [];
                
                foreach (var label in labels)
                {
                    options.Select(KeyFilter.Any, label);
                }
            });
            return builder;
        }
        
        // Add the 
        
        return builder;
    }
    
}