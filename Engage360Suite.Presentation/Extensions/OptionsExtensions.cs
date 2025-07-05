using Engage360Suite.Infrastructure.Configuration;

namespace Engage360Suite.Presentation.Extensions
{
    /// <summary>Fluent helpers to bind & validate strongly-typed options.</summary>
    internal static class OptionsExtensions
    {
        public static IServiceCollection AddPingerbotOptions(
            this IServiceCollection services, IConfiguration cfg) =>
            services.AddOptions<PingerbotOptions>()
                    .Bind(cfg.GetSection("WhatsApp:Pingerbot"))
                    .ValidateOnStart()
                    .Services;

        public static IServiceCollection AddServiceBusOptions(
            this IServiceCollection services, IConfiguration cfg) =>
            services.AddOptions<ServiceBusOptions>()
                    .Bind(cfg.GetSection("ServiceBus"))
                    .ValidateOnStart()
                    .Services;

        public static IServiceCollection AddLeadQueueOptions(
            this IServiceCollection services, IConfiguration cfg) =>
            services.AddOptions<LeadQueueOptions>()
                    .Bind(cfg.GetSection("LeadQueue"))
                    .ValidateOnStart()
                    .Services;
    }
}
