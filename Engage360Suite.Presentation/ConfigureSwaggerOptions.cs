using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Engage360Suite.Presentation
{
    public class ConfigureSwaggerOptions
        : IConfigureOptions<SwaggerGenOptions>
    {
        readonly IApiVersionDescriptionProvider _provider;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) =>
            _provider = provider;

        public void Configure(SwaggerGenOptions options)
        {
            // Add a swagger document for each API version
            foreach (var desc in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(
                    desc.GroupName,
                    new Microsoft.OpenApi.Models.OpenApiInfo()
                    {
                        Title = $"Engage360Suite API {desc.ApiVersion}",
                        Version = desc.ApiVersion.ToString(),
                        Description = "Webhook + WhatsApp integration"
                    });
            }
        }
    }
}
