using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Common.Bootstrapping;

public static class BootstrapExtensions
{
    public static TokenCredential AddTokenCredential(
        this IHostApplicationBuilder builder,
        TokenCredential? credential = null)
    {
        credential ??= new DefaultAzureCredential();
        builder.Services.AddSingleton(credential);
        return credential;
    }
}