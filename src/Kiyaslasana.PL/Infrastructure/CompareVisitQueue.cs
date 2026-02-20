using System.Threading.Channels;
using System.Runtime.CompilerServices;

namespace Kiyaslasana.PL.Infrastructure;

public sealed class CompareVisitQueue : ICompareVisitQueue
{
    private const int QueueCapacity = 1000;

    private readonly Channel<CompareVisitQueueItem> _channel;
    private readonly ILogger<CompareVisitQueue> _logger;
    private int _queuedCount;

    public CompareVisitQueue(ILogger<CompareVisitQueue> logger)
    {
        _logger = logger;

        _channel = Channel.CreateBounded<CompareVisitQueueItem>(new BoundedChannelOptions(QueueCapacity)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.DropOldest
        });
    }

    public bool TryEnqueue(CompareVisitQueueItem item)
    {
        var written = _channel.Writer.TryWrite(item);
        if (!written)
        {
            return false;
        }

        var queued = Interlocked.Increment(ref _queuedCount);
        if (queued > QueueCapacity)
        {
            // DropOldest keeps the queue bounded by evicting one item when full.
            Interlocked.Exchange(ref _queuedCount, QueueCapacity);
            _logger.LogWarning("Compare visit queue is full (capacity {Capacity}); oldest item dropped.", QueueCapacity);
        }

        return true;
    }

    public async IAsyncEnumerable<CompareVisitQueueItem> DequeueAllAsync([EnumeratorCancellation] CancellationToken ct)
    {
        await foreach (var item in _channel.Reader.ReadAllAsync(ct))
        {
            Interlocked.Decrement(ref _queuedCount);
            yield return item;
        }
    }
}
