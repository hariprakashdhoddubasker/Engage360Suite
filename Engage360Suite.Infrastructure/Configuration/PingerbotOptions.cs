using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engage360Suite.Infrastructure.Configuration
{
    /// <summary>
    /// Bind the “WhatsApp:Pingerbot” section of configuration to this type.
    /// </summary>
    public sealed class PingerbotOptions
    {
        /// <summary>
        /// Unique identifier for this instance (for logging or grouping).
        /// </summary>
        public string InstanceId { get; set; } = default!;

        /// <summary>
        /// Bearer token used to authenticate with Pingerbot API.
        /// </summary>
        public string AccessToken { get; set; } = default!;

        /// <summary>
        /// The group ID to which lead notifications should be sent.
        /// </summary>
        public string GroupId { get; set; } = default!;

        /// <summary>
        /// Base URL of the Pingerbot API (e.g. https://pingerbot.in/api/).
        /// </summary>
        public string BaseUrl { get; set; } = default!;
    }
}
