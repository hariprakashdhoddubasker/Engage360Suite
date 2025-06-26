using Engage360Suite.Presentation.Extensions;
using Microsoft.AspNetCore.HttpOverrides;

namespace Engage360Suite.Presentation
{
    public class Startup
    {
        public Startup(IConfiguration config) => Configuration = config;
        public IConfiguration Configuration { get; }

        // Service registration
        public void ConfigureServices(IServiceCollection services)
        {
            // Register application services & infrastructure
            services.AddDefaultServices(Configuration)
                    .Configure<ForwardedHeadersOptions>(opts =>
                    {
                        opts.ForwardedHeaders =
                            ForwardedHeaders.XForwardedFor |
                            ForwardedHeaders.XForwardedProto;
                        opts.KnownNetworks.Clear();
                        opts.KnownProxies.Clear();
                    });
        }

        // HTTP pipeline
        public void Configure(WebApplication app)
        {
            // Configure HTTP middleware pipeline (routing, swagger, static files)
            app.UseApiPipeline();

            // Map health-check endpoint from configuration
            var healthPath = Configuration
                .GetValue<string>("HealthChecks:EndpointPath", "/health");

            app.MapHealthChecks(healthPath);
        }
    }
}
