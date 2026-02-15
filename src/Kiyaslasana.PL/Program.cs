using System.Net;
using System.Net.Sockets;
using Kiyaslasana.BL;
using Kiyaslasana.DAL;
using Kiyaslasana.DAL.Data;
using Kiyaslasana.EL.Entities;
using Kiyaslasana.PL.Infrastructure;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using ForwardedIPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddBusinessLayer();
builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
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

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<KiyaslasanaDbContext>();
    await dbContext.Database.MigrateAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    foreach (var roleName in new[] { "Admin", "Member" })
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}
else
{
    app.UseExceptionHandler("/hata");
    app.UseStatusCodePagesWithReExecute("/hata/{0}");
    app.UseHsts();
}

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program
{
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
