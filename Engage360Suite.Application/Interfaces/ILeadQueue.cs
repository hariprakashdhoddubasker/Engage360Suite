using Engage360Suite.Application.Models;
namespace Engage360Suite.Application.Interfaces
{
    public interface ILeadQueue
    {
        ValueTask EnqueueAsync(LeadDto lead, CancellationToken ct = default);
        IAsyncEnumerable<LeadDto> DequeueAllAsync(CancellationToken ct = default);
    }
}
