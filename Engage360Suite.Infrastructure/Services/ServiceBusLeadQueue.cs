using Azure.Messaging.ServiceBus;
using Engage360Suite.Application.Interfaces;
using Engage360Suite.Application.Models;
using Engage360Suite.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Engage360Suite.Infrastructure.Services
{
    public class ServiceBusLeadQueue : ILeadQueue, IAsyncDisposable
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;
        private readonly ServiceBusProcessor _processor;

        public ServiceBusLeadQueue(
            IOptions<ServiceBusOptions> opts,
            IWhatsAppService whatsApp,            // for dead-letter handling
            ILogger<ServiceBusLeadQueue> logger)
        {
            var options = opts.Value;
            _client = new ServiceBusClient(options.ConnectionString);
            _sender = _client.CreateSender(options.QueueName);

            // Processor will pull from the same queue:
            _processor = _client.CreateProcessor(options.QueueName, new ServiceBusProcessorOptions
            {
                // control concurrency:
                MaxConcurrentCalls = 5,
                AutoCompleteMessages = false
            });

            // wire up event handlers:
            _processor.ProcessMessageAsync += async args =>
            {
                // deserialize
                var lead = JsonSerializer.Deserialize<LeadDto>(
                    args.Message.Body.ToString())!;

                // enqueue into your in-memory channel for BackgroundService:
                await InternalChannel.Writer.WriteAsync(lead, args.CancellationToken);

                // complete the message so it’s removed from SB queue
                await args.CompleteMessageAsync(args.Message, args.CancellationToken);
            };

            _processor.ProcessErrorAsync += args =>
            {
                logger.LogError(args.Exception, "ServiceBus error");
                return Task.CompletedTask;
            };

            // start pumping messages
            _processor.StartProcessingAsync();
        }

        // Under the hood, we still implement ILeadQueue by forwarding to a private channel:
        private readonly Channel<LeadDto> InternalChannel = Channel.CreateUnbounded<LeadDto>();

        // Enqueue pushes into SB
        public async ValueTask EnqueueAsync(LeadDto lead, CancellationToken ct = default)
        {
            var json = JsonSerializer.Serialize(lead);
            var msg = new ServiceBusMessage(json)
            {
                ContentType = "application/json",
                MessageId = Guid.NewGuid().ToString()
            };
            await _sender.SendMessageAsync(msg, ct);
        }

        // DequeueAllAsync pulls from the internal channel as before
        public async IAsyncEnumerable<LeadDto> DequeueAllAsync(
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            while (await InternalChannel.Reader.WaitToReadAsync(ct))
            {
                while (InternalChannel.Reader.TryRead(out var lead))
                    yield return lead;
            }
        }

        // Dispose clients on shutdown
        public async ValueTask DisposeAsync()
        {
            await _processor.DisposeAsync();
            await _sender.DisposeAsync();
            await _client.DisposeAsync();
        }
    }
}
