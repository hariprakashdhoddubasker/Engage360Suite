using Engage360Suite.Application.Models;

namespace Engage360Suite.Application.Interfaces
{
    public interface IWhatsAppService
    {
        Task<SendGroupMessageResult> SendGroupMessageAsync(string message);
    }
}
