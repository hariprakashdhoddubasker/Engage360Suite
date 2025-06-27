using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engage360Suite.Infrastructure.Configuration
{
    /// <summary>
    /// Ensures ServiceBus options (connection string & queue name) are set before startup.
    /// </summary>
    public sealed class ServiceBusOptionsValidator
        : IValidateOptions<ServiceBusOptions>
    {
        public ValidateOptionsResult Validate(string? name, ServiceBusOptions opts)
        {
            if (string.IsNullOrWhiteSpace(opts.ConnectionString))
                return ValidateOptionsResult.Fail(
                    "Configuration error: ServiceBus:ConnectionString must be provided.");

            if (string.IsNullOrWhiteSpace(opts.QueueName))
                return ValidateOptionsResult.Fail(
                    "Configuration error: ServiceBus:QueueName must be provided.");

            return ValidateOptionsResult.Success;
        }
    }
}
