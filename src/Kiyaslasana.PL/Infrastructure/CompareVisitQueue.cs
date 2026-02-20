using System.Threading.Channels;
using Microsoft.Extensions.Options;

namespace Kiyaslasana.PL.Infrastructure;

public sealed class CompareVisitQueue : ICompareVisitQueue
{
    private readonly Channel<CompareVisitQueueItem> _channel;

    public CompareVisitQueue(IOptions<CompareVisitTrackingOptions> options)
    {
        var configuredCapacity = options.Value.QueueCapacity;
        var safeCapacity = Math.Clamp(configuredCapacity, 128, 50_000);

        _channel = Channel.CreateBounded<CompareVisitQueueItem>(new BoundedChannelOptions(safeCapacity)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.DropWrite
        });
    }

    public bool TryEnqueue(CompareVisitQueueItem item)
    {
        return _channel.Writer.TryWrite(item);
    }

    public IAsyncEnumerable<CompareVisitQueueItem> DequeueAllAsync(CancellationToken ct)
    {
        return _channel.Reader.ReadAllAsync(ct);
    }
}
