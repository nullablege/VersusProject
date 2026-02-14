using Kiyaslasana.BL.Abstractions;
using Kiyaslasana.BL.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Kiyaslasana.BL;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLayer(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddScoped<ITelefonService, TelefonService>();
        services.AddSingleton<IAppInfoProvider, AppInfoProvider>();

        return services;
    }
}
