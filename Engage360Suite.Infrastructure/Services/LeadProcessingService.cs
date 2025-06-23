using Engage360Suite.Application.Interfaces;
using Engage360Suite.Infrastructure.Exceptions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Engage360Suite.Infrastructure.Services
{
    public class LeadProcessingService : BackgroundService
    {
        private readonly ILeadQueue _queue;
        private readonly IWhatsAppService _whatsApp;
        private readonly ILogger<LeadProcessingService> _logger;

        public LeadProcessingService(
            ILeadQueue queue,
            IWhatsAppService whatsApp,
            ILogger<LeadProcessingService> logger)
        {
            _queue = queue;
            _whatsApp = whatsApp;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var lead in _queue.DequeueAllAsync(stoppingToken))
            {
                try
                {
                    var message = $"🆕 New Lead:\n{lead.Name}\n{lead.PhoneNumber}";
                    await _whatsApp.SendGroupMessageAsync(message, stoppingToken);
                    _logger.LogInformation("Processed lead {Name}/{Phone}", lead.Name, lead.PhoneNumber);
                }
                catch (WhatsAppException ex)
                {
                    _logger.LogError(ex, "Failed to process lead {Name}", lead.Name);
                }
            }
        }
    }
}
