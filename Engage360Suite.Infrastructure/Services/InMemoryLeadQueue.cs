using Engage360Suite.Application.Interfaces;
using Engage360Suite.Application.Models;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Engage360Suite.Infrastructure.Configuration;

namespace Engage360Suite.Infrastructure.Services
{
    /// <summary>
    /// In-memory bounded queue for leads. Use in single-node deployments or tests;
    /// switch to Service Bus implementation for multi-node scaling.
    /// </summary>
    public sealed class InMemoryLeadQueue : ILeadQueue
    {
        private readonly Channel<LeadDto> _channel;
        private readonly ILogger<InMemoryLeadQueue> _logger;
        private readonly Counter<int> _enqueueCounter;
        private readonly Histogram<int> _queueDepth;

        public InMemoryLeadQueue(
            IOptions<LeadQueueOptions> options,
            ILogger<InMemoryLeadQueue> logger,
            IMeterFactory meterFactory)
        {
            var capacity = Math.Max(options.Value.Capacity, 1);
            _channel = Channel.CreateBounded<LeadDto>(new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = false,
                SingleWriter = false
            });

            _logger = logger;

            var meter = meterFactory.Create("Engage360Suite.Infrastructure");
            _enqueueCounter = meter.CreateCounter<int>("leadqueue_enqueued_total");
            _queueDepth = meter.CreateHistogram<int>("leadqueue_depth");
        }

        public async ValueTask EnqueueAsync(LeadDto lead, CancellationToken ct = default)
        {
            try
            {
                await _channel.Writer.WriteAsync(lead, ct).ConfigureAwait(false);
                _enqueueCounter.Add(1);
                _queueDepth.Record(_channel.Reader.Count);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                _logger.LogWarning("Enqueue cancelled.");
            }
            catch (ChannelClosedException ex)
            {
                _logger.LogError(ex, "Lead queue writer closed – lead dropped.");
                throw;
            }
        }

        public async IAsyncEnumerable<LeadDto> DequeueAllAsync(
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            while (await _channel.Reader.WaitToReadAsync(ct).ConfigureAwait(false))
            {
                while (_channel.Reader.TryRead(out var lead))
                {
                    _queueDepth.Record(_channel.Reader.Count);
                    yield return lead;
                }
            }
        }
    }

}
