using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace Engage360Suite.Infrastructure.Filters
{
    /// <summary>
    /// Rejects any request that does not supply the correct X-API-KEY header.
    /// </summary>
    public class ApiKeyActionFilter : IAsyncActionFilter
    {
        private const string HeaderName = "X-API-KEY";
        private readonly string _expectedApiKey;

        public ApiKeyActionFilter(IConfiguration config)
        {
            // Read the key from configuration (User-Secrets, appsettings, or env-vars)
            _expectedApiKey = config["Webhook:ApiKey"]
                ?? throw new ArgumentException("Missing configuration: Webhook:ApiKey");
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var req = context.HttpContext.Request;

            // 1. Check header exists
            if (!req.Headers.TryGetValue(HeaderName, out var providedKey))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // 2. Validate the key
            if (!string.Equals(providedKey, _expectedApiKey, StringComparison.Ordinal))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // 3. All good → continue into the action
            await next();
        }
    }
}
