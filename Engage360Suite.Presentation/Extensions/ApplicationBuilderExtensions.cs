using Asp.Versioning.ApiExplorer;
using Engage360Suite.Presentation.Middlewares;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Engage360Suite.Presentation.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Sets up forwarded headers, Swagger UI, static files, routing,
        /// and maps controller & static endpoints.
        /// </summary>
        public static WebApplication UseApiPipeline(this WebApplication app, IConfiguration config)
        {
            app.UseForwardedHeaders();
            app.UseMiddleware<GlobalExceptionMiddleware>();
            app.UseSwaggerAccordingToEnvironment()
               .UseStaticAndRouting()
               .MapAppEndpoints()
               .UseOpenTelemetryPrometheusScrapingEndpoint();

            // Map health-check endpoint from configuration
            var healthPath = config.GetValue<string>("HealthChecks:EndpointPath", "/health");
            app.MapHealthChecks(healthPath);
            return app;
        }

        public static WebApplication UseSwaggerAccordingToEnvironment(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                // In Development: serve Swagger JSON/docs
                var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    foreach (var desc in provider.ApiVersionDescriptions)
                    {
                        c.SwaggerEndpoint(
                            $"/swagger/{desc.GroupName}/swagger.json",
                            desc.GroupName.ToUpperInvariant());
                    }
                });
            }
            else
            {
                // In Production: global exception handler & HSTS
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            return app;
        }

        public static WebApplication UseStaticAndRouting(this WebApplication app)
        {
            app.UseHttpsRedirection()
               .UseSerilogRequestLogging()
               .UseStaticFiles()
               .UseRouting()
               .UseAuthorization();

            return app;
        }

        public static WebApplication MapAppEndpoints(this WebApplication app)
        {
            app.MapHealthChecks("/health");
            app.MapControllers();
            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();
            return app;
        }
    }
}
