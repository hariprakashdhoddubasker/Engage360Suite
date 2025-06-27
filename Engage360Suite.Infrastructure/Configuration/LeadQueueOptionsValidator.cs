using Engage360Suite.Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace Engage360Suite.Infrastructure.Configuration
{
    /// <summary>
    /// Ensures <see cref="LeadQueueOptions"/> is valid before the
    /// application finishes starting.
    /// </summary>
    public sealed class LeadQueueOptionsValidator : IValidateOptions<LeadQueueOptions>
    {
        public ValidateOptionsResult Validate(string? name, LeadQueueOptions opts) =>
            opts.Capacity <= 0
                ? ValidateOptionsResult.Fail("LeadQueueOptions: Capacity must be greater than 0.")
                : ValidateOptionsResult.Success;
    }
}
