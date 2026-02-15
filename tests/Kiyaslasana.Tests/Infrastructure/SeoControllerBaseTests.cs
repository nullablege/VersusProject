using Kiyaslasana.PL.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Kiyaslasana.Tests.Infrastructure;

public class SeoControllerBaseTests
{
    [Fact]
    public void BuildAbsoluteUrl_UsesConfiguredPublicBaseUrl()
    {
        var controller = new TestSeoController();
        var context = new DefaultHttpContext();
        context.Request.Scheme = "http";
        context.Request.Host = new HostString("spoofed.example");
        context.Request.PathBase = "/app";
        context.RequestServices = BuildServices(
            environmentName: Environments.Production,
            publicBaseUrl: "https://kiyaslasana.com");

        controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = context
        };

        var absoluteUrl = controller.BuildUrl("/telefonlar");

        Assert.Equal("https://kiyaslasana.com/app/telefonlar", absoluteUrl);
    }

    private static IServiceProvider BuildServices(string environmentName, string? publicBaseUrl)
    {
        var configValues = new Dictionary<string, string?>();
        if (publicBaseUrl is not null)
        {
            configValues["Seo:PublicBaseUrl"] = publicBaseUrl;
        }

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<IHostEnvironment>(new TestHostEnvironment(environmentName));

        return services.BuildServiceProvider();
    }

    private sealed class TestSeoController : SeoControllerBase
    {
        public string BuildUrl(string relativePath)
        {
            return BuildAbsoluteUrl(relativePath);
        }
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public TestHostEnvironment(string environmentName)
        {
            EnvironmentName = environmentName;
        }

        public string EnvironmentName { get; set; }

        public string ApplicationName { get; set; } = "Kiyaslasana.Tests";

        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();

        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } =
            new Microsoft.Extensions.FileProviders.NullFileProvider();
    }
}
