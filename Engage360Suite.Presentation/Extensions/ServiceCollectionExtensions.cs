using Asp.Versioning;
using Engage360Suite.Application.Interfaces;
using Engage360Suite.Application.Models;
using Engage360Suite.Infrastructure.Configuration;
using Engage360Suite.Infrastructure.Filters;
using Engage360Suite.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Engage360Suite.Presentation.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddVersioningAndSwagger(this IServiceCollection services)
        {
            services
              .AddApiVersioning(o =>
              {
                  o.DefaultApiVersion = new ApiVersion(1, 0);
                  o.AssumeDefaultVersionWhenUnspecified = true;
                  o.ReportApiVersions = true;
                  o.ApiVersionReader = new UrlSegmentApiVersionReader();
              })
              .AddApiExplorer(o =>
              {
                  o.GroupNameFormat = "'v'VVV";
                  o.SubstituteApiVersionInUrl = true;
              });

            // your existing ConfigureSwaggerOptions must be registered first
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
                {
                    Description = "X-API-KEY header",
                    Name = "X-API-KEY",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                     Scheme = "ApiKeyScheme"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "ApiKey"
                        }
                    }
                  ] = new string[] { }
                });
            });

            return services;
        }

        /// <summary>
        /// Registers MVC controllers/views and applies the API-key filter globally.
        /// </summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddScoped<ApiKeyActionFilter>();

            return services;
        }

        /// <summary>
        /// Binds Pingerbot & ServiceBus options, configures HttpClient resiliency,
        /// and registers the distributed queue + background worker.
        /// </summary>
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Bind options from configuration
            services.Configure<PingerbotOptions>(configuration.GetSection("WhatsApp:Pingerbot"));
            services.Configure<ServiceBusOptions>(configuration.GetSection("ServiceBus"));

            // HttpClient for IWhatsAppService with Polly retry + circuit-breaker
            services.AddHttpClient<IWhatsAppService, WhatsAppService>(client =>
              {
                  client.BaseAddress = new Uri("https://pingerbot.in/api/");
              });

            // Distributed queue + background worker
            // services.AddSingleton<ILeadQueue, InMemoryLeadQueue>();
            services.AddSingleton<ILeadQueue, ServiceBusLeadQueue>();
            services.AddHostedService<LeadProcessingService>();

            return services;
        }
    }
}
