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
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
        | ForwardedHeaders.XForwardedProto
        | ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
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

app.UseForwardedHeaders();
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
}
