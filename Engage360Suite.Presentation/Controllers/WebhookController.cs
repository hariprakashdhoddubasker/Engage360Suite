using Asp.Versioning;
using Engage360Suite.Application.Interfaces;
using Engage360Suite.Application.Models;
using Engage360Suite.Infrastructure.Exceptions;
using Engage360Suite.Infrastructure.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Engage360Suite.Presentation.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ServiceFilter(typeof(ApiKeyActionFilter))]
    public class WebhookController : ControllerBase
    {
        private readonly ILeadQueue _queue;

        public WebhookController(ILeadQueue queue) => _queue = queue;

        [HttpPost("lead")]
        public async Task<IActionResult> Lead([FromBody] LeadDto lead, CancellationToken ct)
        {
            try
            {
                await _queue.EnqueueAsync(lead, ct);
                return Accepted(new { enqueued = true });
            }
            catch (WhatsAppException ex)
            {
                return StatusCode(502, new { error = ex.Message });
            }
        }
    }
}
