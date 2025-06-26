namespace Engage360Suite.Presentation.Extensions
{
    /// <summary>
    /// Extension methods for configuring application-wide settings.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Adds Serilog-specific JSON file and environment variables
        /// as configuration sources (with reload-on-change).
        /// </summary>
        /// <param name="builder">The WebApplicationBuilder to configure.</param>
        /// <returns>The same WebApplicationBuilder for chaining.</returns>
        public static WebApplicationBuilder ConfigureAppSettings(this WebApplicationBuilder builder)
        {
            builder.Configuration
                   .AddJsonFile("serilog.json", optional: true, reloadOnChange: true)
                   .AddEnvironmentVariables();
            return builder;
        }
    }
}
