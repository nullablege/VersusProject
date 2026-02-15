using Kiyaslasana.BL.Abstractions;
using Kiyaslasana.BL.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Kiyaslasana.BL;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLayer(this IServiceCollection services)
    {
        services.AddMemoryCache(options =>
        {
            // Hard limit to reduce unbounded growth from high-cardinality cache keys.
            options.SizeLimit = 5000;
        });
        services.AddScoped<ITelefonService, TelefonService>();
        services.AddScoped<ITelefonSitemapQuery, TelefonSitemapQuery>();
        services.AddSingleton<IAppInfoProvider, AppInfoProvider>();

        return services;
    }
}
