using Engage360Suite.Application.Interfaces;
using Engage360Suite.Application.Models;
using Engage360Suite.Infrastructure.Exceptions;
using Engage360Suite.Infrastructure.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Engage360Suite.Presentation.Controllers
{
    [ApiController]
    [Route("api/webhook")]
    [ServiceFilter(typeof(ApiKeyActionFilter))]
    public class WebhookController : ControllerBase
    {
        private readonly IWhatsAppService _whatsApp;

        public WebhookController(IWhatsAppService whatsApp)
        {
            _whatsApp = whatsApp;
        }

        [HttpPost("lead")]
        public async Task<IActionResult> Lead([FromBody] LeadDto lead)
        {
            try
            {
                await _whatsApp.SendGroupMessageAsync($"🆕New lead:\n{lead.Name}\n{lead.PhoneNumber}");
                return Ok(new { success = true });
            }
            catch (WhatsAppException ex)
            {
                return StatusCode(502, new { error = ex.Message });
            }
        }
    }
}
