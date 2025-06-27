using Asp.Versioning.ApiExplorer;

namespace Engage360Suite.Presentation.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Sets up forwarded headers, Swagger UI, static files, routing,
        /// and maps controller & static endpoints.
        /// </summary>
        public static WebApplication UseApiPipeline(this WebApplication app)
        {
            app.UseForwardedHeaders();
            app.UseSwaggerAccordingToEnvironment()
               .UseStaticAndRouting()
               .MapAppEndpoints()
               .UseOpenTelemetryPrometheusScrapingEndpoint();
            app.MapHealthChecks("/health");
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
               .UseStaticFiles()
               .UseRouting()
               .UseAuthorization();

            return app;
        }

        public static WebApplication MapAppEndpoints(this WebApplication app)
        {
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
