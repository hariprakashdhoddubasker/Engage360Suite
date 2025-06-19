using Engage360Suite.Application.Interfaces;
using Engage360Suite.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace Engage360Suite.Presentation.Controllers
{
    [ApiController]
    [Route("api/webhook")]
    public class WebhookController : Controller
    {
        private readonly IWhatsAppService _whatsApp;
        
        public WebhookController(IWhatsAppService whatsApp)
        {
            _whatsApp = whatsApp;
        }

        [HttpPost("lead")]
        public async Task<IActionResult> Lead([FromBody] LeadDto lead)
        {
            // use the injected service
            await _whatsApp.SendGroupMessageAsync($"New lead: {lead.Name} ({lead.PhoneNumber})"
            );
            return Ok(new { success = true });
        }
    }
}
