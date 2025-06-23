using Engage360Suite.Application.Interfaces;
using Engage360Suite.Application.Models;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Engage360Suite.Infrastructure.Services
{
    public class InMemoryLeadQueue : ILeadQueue
    {
        private readonly Channel<LeadDto> _channel = Channel.CreateUnbounded<LeadDto>();

        public async ValueTask EnqueueAsync(LeadDto lead, CancellationToken ct = default)
        {
            await _channel.Writer.WriteAsync(lead, ct);
        }

        public async IAsyncEnumerable<LeadDto> DequeueAllAsync(
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            while (await _channel.Reader.WaitToReadAsync(ct))
            {
                while (_channel.Reader.TryRead(out var lead))
                    yield return lead;
            }
        }
    }
}
