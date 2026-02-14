using System.Reflection;
using Kiyaslasana.BL.Abstractions;
using Kiyaslasana.BL.Contracts;

namespace Kiyaslasana.BL.Services;

public sealed class AppInfoProvider : IAppInfoProvider
{
    private readonly DateTimeOffset _startedAtUtc = DateTimeOffset.UtcNow;

    public SystemInfoDto GetInfo()
    {
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var assemblyName = assembly.GetName();

        return new SystemInfoDto(
            Name: assemblyName.Name ?? "Kiyaslasana",
            Version: assemblyName.Version?.ToString() ?? "unknown",
            StartedAtUtc: _startedAtUtc);
    }
}
