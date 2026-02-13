using System.Reflection;
using Versus.Contracts;

namespace Versus.Services
{
    public sealed class AppInfoProvider : IAppInfoProvider
    {
        private readonly DateTimeOffset _startedAtUtc = DateTimeOffset.UtcNow;

        public SystemInfoDto GetInfo()
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            var assemblyName = assembly.GetName();

            return new SystemInfoDto(
                Name: assemblyName.Name ?? "Versus",
                Version: assemblyName.Version?.ToString() ?? "unknown",
                StartedAtUtc: _startedAtUtc);
        }
    }
}
