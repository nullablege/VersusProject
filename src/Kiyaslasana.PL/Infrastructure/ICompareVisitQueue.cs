namespace Kiyaslasana.PL.Infrastructure;

public interface ICompareVisitQueue
{
    bool TryEnqueue(CompareVisitQueueItem item);

    IAsyncEnumerable<CompareVisitQueueItem> DequeueAllAsync(CancellationToken ct);
}
