using Azure.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Common.Bootstrapping;

public static class HostConfigurationExtensions
{
    private const string LocalDevConfigFile = "appsettings.local.json";
    private const string KubeConfigFile = "/service/configs/appsettings.k8s.json";
    
    public static IHostApplicationBuilder AddConfiguration(
        this IHostApplicationBuilder builder,
        TokenCredential tokenCredential)
    {
        // Add the app settings used when developing locally.   When the service is deployed
        // to Kubernetes, configuration as loaded by the Azure configuration provider and this
        // local file is excluded within the .dockerignore when the image is built.
        builder.Configuration.AddJsonFile(LocalDevConfigFile, optional: true);
        builder.Configuration.AddJsonFile(KubeConfigFile, optional: true);
        
        // Add the token credential to the container so it can be used when registering other azure services.
        builder.Services.AddSingleton(tokenCredential);
        
        var endpoint = builder.Configuration["Service:AppConfig:Endpoint"];
        if (endpoint is null)
        {
            if (!File.Exists(KubeConfigFile))
            {
                throw new InvalidOperationException(
                    $"No Local App Configuration endpoint configured, and no Kubernetes config file found at ${KubeConfigFile}. " +
                    "Expected to find Local App Configuration endpoint in configuration key 'Service:AppConfig:Endpoint'");
            }
            
            return builder;
        }
        
        // Add Azure App configuration for local development:
        builder.Configuration.AddAzureAppConfiguration(options =>
        {
            options.Connect(new Uri(endpoint), tokenCredential);
            options.ConfigureKeyVault(kv => kv.SetCredential(tokenCredential));
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
}