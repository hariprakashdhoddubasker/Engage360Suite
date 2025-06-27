using Asp.Versioning;
using Engage360Suite.Application.Interfaces;
using Engage360Suite.Infrastructure.Configuration;
using Engage360Suite.Infrastructure.Filters;
using Engage360Suite.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Polly;
using OpenTelemetry.Metrics;

namespace Engage360Suite.Presentation.Extensions
{
    /// <summary>
    /// Extension methods for registering application services in DI.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers core services: health checks, API versioning, Swagger,
        /// application services, and infrastructure services.
        /// </summary>
        /// <param name="services">The IServiceCollection to populate.</param>
        /// <param name="config">The application IConfiguration.</param>
        /// <returns>The same IServiceCollection for chaining.</returns>
        public static IServiceCollection AddDefaultServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddHealthChecks();
            services.AddVersioningAndSwagger();
            services.AddApplicationServices();
            services.AddInfrastructureServices(config);
            return services;
        }

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
            services.AddOptions<PingerbotOptions>()
                    .Bind(configuration.GetSection("WhatsApp:Pingerbot"))
                    .ValidateOnStart();

            services.AddOptions<ServiceBusOptions>()
                    .Bind(configuration.GetSection("ServiceBus"))
                    .ValidateOnStart();

            services.AddOptions<LeadQueueOptions>()
                    .Bind(configuration.GetSection("LeadQueue"))
                    .ValidateOnStart();

            services.AddSingleton<IValidateOptions<PingerbotOptions>, PingerbotOptionsValidator>();
            services.AddSingleton<IValidateOptions<ServiceBusOptions>, ServiceBusOptionsValidator>();
            services.AddSingleton<IValidateOptions<LeadQueueOptions>, LeadQueueOptionsValidator>();

            // Conditional queue implementation
            if (configuration.GetValue<bool>("ServiceBus:Enabled"))
                services.AddSingleton<ILeadQueue, ServiceBusLeadQueue>();   // prod / cloud
            else
                services.AddSingleton<ILeadQueue, InMemoryLeadQueue>();     // dev / single-node

            services.AddHttpClient<IWhatsAppService, WhatsAppService>((sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<PingerbotOptions>>().Value;
                client.BaseAddress = new Uri(opts.BaseUrl);             // now configurable
                client.Timeout = TimeSpan.FromSeconds(10);
            })
            .AddTransientHttpErrorPolicy(p =>                       // retry 3× w/ decorrelated jitter back-off
                p.WaitAndRetryAsync(3, attempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, attempt))))
            .AddTransientHttpErrorPolicy(p =>                       // circuit breaker after 5 faults / 30 s
                p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));

            // Background worker
            services.AddHostedService<LeadProcessingService>();

            // Observability
            services.AddOpenTelemetry()
                    .WithMetrics(builder =>
                    {
                        builder.AddMeter("Engage360Suite.Infrastructure"); // ← matches InMemoryLeadQueue
                        builder.AddPrometheusExporter();                   // or Azure Monitor exporter
                    });
            return services;
        }
    }
}
