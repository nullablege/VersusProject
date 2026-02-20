using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Data.Common;
using Kiyaslasana.BL;
using Kiyaslasana.DAL;
using Kiyaslasana.DAL.Data;
using Kiyaslasana.EL.Entities;
using Kiyaslasana.PL.Areas.Admin.Models;
using Kiyaslasana.PL.Areas.Admin.Validators;
using Kiyaslasana.PL.ViewModels.Auth;
using Kiyaslasana.PL.Validators.Auth;
using Kiyaslasana.PL.Infrastructure;
using FluentValidation.AspNetCore;
using FluentValidation;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Net.Http.Headers;
using ForwardedIPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<CspNonceViewDataFilter>();
});
builder.Services.AddRazorPages();
builder.Services.Configure<RouteOptions>(options =>
{
    options.ConstraintMap["telefonslug"] = typeof(TelefonSlugRouteConstraint);
});
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddScoped<IValidator<BlogPostEditorInputModel>, BlogPostEditorInputValidator>();
builder.Services.AddScoped<IValidator<TelefonReviewEditorInputModel>, TelefonReviewEditorInputValidator>();
builder.Services.AddScoped<IValidator<LoginViewModel>, LoginValidator>();
builder.Services.AddScoped<IValidator<RegisterViewModel>, RegisterValidator>();

builder.Services.AddBusinessLayer();
builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<KiyaslasanaDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    // Security: never trust client-controlled host forwarding headers.
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
        | ForwardedHeaders.XForwardedProto;

    if (builder.Environment.IsDevelopment())
    {
        // Development convenience for local reverse proxies/tunnels.
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
        return;
    }

    // Production: trust only explicitly configured reverse proxy hops.
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();

    var trustedProxies = builder.Configuration.GetSection("ReverseProxy:TrustedProxies").Get<string[]>() ?? [];
    foreach (var trustedProxy in trustedProxies)
    {
        if (IPAddress.TryParse(trustedProxy, out var proxyAddress))
        {
            options.KnownProxies.Add(proxyAddress);
        }
    }

    var trustedNetworks = builder.Configuration.GetSection("ReverseProxy:TrustedNetworks").Get<string[]>() ?? [];
    foreach (var trustedNetwork in trustedNetworks)
    {
        if (TryParseCidr(trustedNetwork, out var network))
        {
            options.KnownNetworks.Add(network);
        }
    }
});

builder.Services.AddOutputCache(options =>
{
    options.AddPolicy(OutputCachePolicyNames.AnonymousOneHour, policy =>
    {
        policy
            .Expire(TimeSpan.FromHours(1))
            .SetVaryByHost(true)
            .With(context => !(context.HttpContext.User.Identity?.IsAuthenticated ?? false));
    });

    options.AddPolicy(OutputCachePolicyNames.AnonymousOneDay, policy =>
    {
        policy
            .Expire(TimeSpan.FromHours(24))
            .SetVaryByHost(true)
            .With(context => !(context.HttpContext.User.Identity?.IsAuthenticated ?? false));
    });
});

var app = builder.Build();
var applyMigrationsOnStartup = builder.Configuration.GetValue<bool?>("Database:ApplyMigrationsOnStartup")
    ?? builder.Environment.IsDevelopment();

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/error/{0}");

app.Use(async (context, next) =>
{
    var cspNonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    context.Items[CspNonceViewDataFilter.CspNonceItemKey] = cspNonce;

    context.Response.OnStarting(() =>
    {
        context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
        context.Response.Headers.TryAdd("X-Frame-Options", "SAMEORIGIN");
        context.Response.Headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.TryAdd("Permissions-Policy", "geolocation=(), camera=(), microphone=()");

        var csp = "default-src 'self'; " +
            "object-src 'none'; " +
            "base-uri 'self'; " +
            "frame-ancestors 'self'; " +
            $"script-src 'self' 'nonce-{cspNonce}' https://cdn.jsdelivr.net; " +
            "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdn.jsdelivr.net; " +
            "img-src 'self' data: https:; " +
            "font-src 'self' data: https://fonts.gstatic.com https://cdn.jsdelivr.net";
        context.Response.Headers.TryAdd("Content-Security-Policy", csp);

        return Task.CompletedTask;
    });

    await next();
});

await InitializePersistenceAsync(app.Services, app.Configuration, applyMigrationsOnStartup);

app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseOutputCache();

app.Use(async (context, next) =>
{
    await next();

    var isAnonymousGet = HttpMethods.IsGet(context.Request.Method)
        && !(context.User.Identity?.IsAuthenticated ?? false)
        && context.Response.StatusCode == StatusCodes.Status200OK;

    if (!isAnonymousGet)
    {
        return;
    }

    if (context.Response.Headers.ContainsKey(HeaderNames.CacheControl))
    {
        return;
    }

    var isHtml = context.Response.ContentType?.Contains("text/html", StringComparison.OrdinalIgnoreCase) ?? false;
    if (!isHtml)
    {
        return;
    }

    context.Response.Headers[HeaderNames.CacheControl] = "public, max-age=120, s-maxage=3600, stale-while-revalidate=300";
});

app.MapRazorPages();
app.MapControllers();

app.MapGet("/health", async Task<IResult> (
    KiyaslasanaDbContext dbContext,
    IMemoryCache memoryCache,
    CancellationToken ct) =>
{
    var databaseHealthy = false;
    try
    {
        databaseHealthy = await dbContext.Database.CanConnectAsync(ct);
    }
    catch
    {
        databaseHealthy = false;
    }

    var memoryCacheHealthy = false;
    try
    {
        const string healthCacheKey = "health:memory-cache:v1";
        memoryCache.Set(healthCacheKey, "ok", TimeSpan.FromSeconds(30));
        memoryCacheHealthy = memoryCache.TryGetValue<string>(healthCacheKey, out var value)
            && string.Equals(value, "ok", StringComparison.Ordinal);
    }
    catch
    {
        memoryCacheHealthy = false;
    }

    var allHealthy = databaseHealthy && memoryCacheHealthy;
    var response = new
    {
        status = allHealthy ? "Healthy" : "Unhealthy",
        checks = new Dictionary<string, string>
        {
            ["database"] = databaseHealthy ? "Healthy" : "Unhealthy",
            ["memory_cache"] = memoryCacheHealthy ? "Healthy" : "Unhealthy"
        }
    };

    return Results.Json(
        response,
        statusCode: allHealthy
            ? StatusCodes.Status200OK
            : StatusCodes.Status503ServiceUnavailable);
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program
{
    private static async Task InitializePersistenceAsync(
        IServiceProvider services,
        IConfiguration configuration,
        bool applyMigrationsOnStartup)
    {
        if (applyMigrationsOnStartup)
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<KiyaslasanaDbContext>();
            await dbContext.Database.MigrateAsync();
        }

        try
        {
            await IdentitySeeder.SeedAsync(services, configuration);
        }
        catch (Exception ex) when (!applyMigrationsOnStartup && IsPotentialDatabaseReadinessIssue(ex))
        {
            throw new InvalidOperationException(
                "Identity seed failed because the database schema may be missing. " +
                "Set 'Database:ApplyMigrationsOnStartup' to true or run migrations before startup.",
                ex);
        }
    }

    private static bool IsPotentialDatabaseReadinessIssue(Exception ex)
    {
        if (ex is DbException || ex.InnerException is DbException)
        {
            return true;
        }

        return ex.Message.Contains("Invalid object name", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("does not exist", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("no such table", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryParseCidr(string cidr, out ForwardedIPNetwork network)
    {
        network = null!;

        if (string.IsNullOrWhiteSpace(cidr))
        {
            return false;
        }

        var parts = cidr.Split('/', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2 || !IPAddress.TryParse(parts[0], out var baseAddress))
        {
            return false;
        }

        if (!int.TryParse(parts[1], out var prefixLength))
        {
            return false;
        }

        var maxPrefix = baseAddress.AddressFamily == AddressFamily.InterNetwork ? 32 :
            baseAddress.AddressFamily == AddressFamily.InterNetworkV6 ? 128 :
            -1;

        if (maxPrefix < 0 || prefixLength < 0 || prefixLength > maxPrefix)
        {
            return false;
        }

        network = new ForwardedIPNetwork(baseAddress, prefixLength);
        return true;
    }
}
