using Engage360Suite.Application.Interfaces;
using Engage360Suite.Application.Models;
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
        public async Task<SendGroupMessageResult> SendGroupMessageAsync(string textMessage)
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
            var response = await _http.PostAsync("https://pingerbot.in/api/send_group", content);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<SendGroupMessageResult>(jsonResponse);

            if (result == null)
                throw new InvalidOperationException("Failed to parse Pingerbot response.");

            return result;
        }
    }
}
