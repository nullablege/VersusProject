using System.Security.Cryptography;
using System.Text;
using Kiyaslasana.BL.Abstractions;
using Microsoft.Extensions.Options;

namespace Kiyaslasana.PL.Infrastructure;

public sealed class CompareVisitBackgroundService : BackgroundService
{
    private readonly ICompareVisitQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<CompareVisitTrackingOptions> _options;
    private readonly ILogger<CompareVisitBackgroundService> _logger;

    public CompareVisitBackgroundService(
        ICompareVisitQueue queue,
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<CompareVisitTrackingOptions> options,
        ILogger<CompareVisitBackgroundService> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var warnedForMissingSalt = false;

        await foreach (var item in _queue.DequeueAllAsync(stoppingToken))
        {
            try
            {
                var salt = (_options.CurrentValue.IpHashSalt ?? string.Empty).Trim();
                if (salt.Length == 0)
                {
                    if (!warnedForMissingSalt)
                    {
                        warnedForMissingSalt = true;
                        _logger.LogWarning(
                            "CompareVisit tracking is disabled because '{Section}:{Key}' is empty.",
                            CompareVisitTrackingOptions.SectionName,
                            nameof(CompareVisitTrackingOptions.IpHashSalt));
                    }

                    continue;
                }

                var ipHash = BuildIpHash(item.RemoteIpAddress, salt);

                using var scope = _scopeFactory.CreateScope();
                var telefonService = scope.ServiceProvider.GetRequiredService<ITelefonService>();
                await telefonService.RecordCompareVisitAsync(
                    item.CanonicalLeftSlug,
                    item.CanonicalRightSlug,
                    ipHash,
                    stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to persist compare visit for {CanonicalLeftSlug}|{CanonicalRightSlug}",
                    item.CanonicalLeftSlug,
                    item.CanonicalRightSlug);
            }
        }
    }

    private static string? BuildIpHash(string? remoteIpAddress, string salt)
    {
        if (string.IsNullOrWhiteSpace(remoteIpAddress))
        {
            return null;
        }

        var bytes = Encoding.UTF8.GetBytes($"{remoteIpAddress.Trim()}{salt}");
        return Convert.ToHexString(SHA256.HashData(bytes));
    }
}
