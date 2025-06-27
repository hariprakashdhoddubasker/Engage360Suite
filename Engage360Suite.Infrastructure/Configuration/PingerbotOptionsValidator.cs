using Engage360Suite.Application.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engage360Suite.Infrastructure.Configuration
{
    /// <summary>
    /// Validates that all required WhatsApp:Pingerbot settings are provided..
    /// </summary>
    public sealed class PingerbotOptionsValidator : IValidateOptions<PingerbotOptions>
    {
        public ValidateOptionsResult Validate(string? name, PingerbotOptions opts)
        {
            if (string.IsNullOrWhiteSpace(opts.BaseUrl))
                return ValidateOptionsResult.Fail("BaseUrl must be provided.");

            if (!Uri.TryCreate(opts.BaseUrl, UriKind.Absolute, out var uri)
                    || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
                return ValidateOptionsResult.Fail("BaseUrl must be a valid http/https URI.");

            if (string.IsNullOrWhiteSpace(opts.AccessToken))
            {
                return ValidateOptionsResult.Fail(
                    "Configuration error: WhatsApp:Pingerbot:AccessToken must be provided.");
            }

            if (string.IsNullOrWhiteSpace(opts.GroupId))
            {
                return ValidateOptionsResult.Fail(
                    "Configuration error: WhatsApp:Pingerbot:GroupId must be provided.");
            }

            if (string.IsNullOrWhiteSpace(opts.InstanceId))
            {
                return ValidateOptionsResult.Fail(
                    "Configuration error: WhatsApp:Pingerbot:InstanceId must be provided.");
            }

            return ValidateOptionsResult.Success;
        }
    }
}
