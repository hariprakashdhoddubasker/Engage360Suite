using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engage360Suite.Infrastructure.Configuration
{
    /// <summary>
    /// Binds to the "ServiceBus" section of your configuration (appsettings.json, env-vars, user-secrets).
    /// </summary>
    public class ServiceBusOptions
    {
        /// <summary>
        /// The full connection string to your Azure Service Bus namespace.
        /// </summary>
        public string ConnectionString { get; set; } = default!;

        /// <summary>
        /// The queue name to which Lead messages should be sent.
        /// </summary>
        public string QueueName { get; set; } = default!;
    }
}
