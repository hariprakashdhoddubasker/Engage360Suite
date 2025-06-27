using Engage360Suite.Application.Interfaces;
using Engage360Suite.Application.Models;
using Engage360Suite.Infrastructure.Configuration;
using Engage360Suite.Infrastructure.Exceptions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace Engage360Suite.Infrastructure.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly HttpClient _http;
        private readonly PingerbotOptions _opts;
        public WhatsAppService(HttpClient http, IOptions<PingerbotOptions> optsAccessor)
        {
            _http = http;
            _opts = optsAccessor.Value;
        }
        public async Task<SendGroupMessageResult> SendGroupMessageAsync(string textMessage,
            CancellationToken cancellationToken = default)
        {
            var payload = new
            {
                group_id = _opts.GroupId,
                type = "text",
                message = textMessage,
                instance_id = _opts.InstanceId,
                access_token = _opts.AccessToken
            };
            var jsonPayload = JsonConvert.SerializeObject(payload);
            using var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync("https://pingerbot.in/api/send_group", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<SendGroupMessageResult>(jsonResponse)
                             ?? throw new WhatsAppException("Failed to parse Pingerbot response");

            if (!string.Equals(result.Status, "success", StringComparison.OrdinalIgnoreCase))
            {
                throw new WhatsAppException($"Pingerbot API error: {result.Message.Key.Id}");
            }

            return result;
        }
    }
}
