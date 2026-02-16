using Kiyaslasana.BL.Abstractions;
using Kiyaslasana.DAL.Data;
using Kiyaslasana.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kiyaslasana.DAL;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccessLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<KiyaslasanaDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("Default"));
            options.UseSnakeCaseNamingConvention();
        });

        services.AddScoped<ITelefonRepository, EfTelefonRepository>();
        services.AddScoped<ITelefonReviewRepository, EfTelefonReviewRepository>();
        services.AddScoped<IBlogPostRepository, EfBlogPostRepository>();

        return services;
    }
}
