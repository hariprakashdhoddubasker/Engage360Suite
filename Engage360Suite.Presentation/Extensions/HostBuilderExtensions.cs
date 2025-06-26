using Serilog;

namespace Engage360Suite.Presentation.Extensions
{
    /// <summary>
    /// Extension methods for configuring the IHostBuilder.
    /// </summary>
    public static class HostBuilderExtensions
    {
        /// <summary>
        /// Integrates Serilog into the generic host, reading settings from
        /// IConfiguration and the DI container.
        /// </summary>
        /// <param name="host">The IHostBuilder being configured.</param>
        /// <returns>The same IHostBuilder for chaining.</returns>
        public static IHostBuilder UseObservability(this IHostBuilder host) =>
            host.UseSerilog((ctx, svc, cfg) =>
                cfg.ReadFrom.Configuration(ctx.Configuration)
                   .ReadFrom.Services(svc)
                   .Enrich.FromLogContext());
    }
}
