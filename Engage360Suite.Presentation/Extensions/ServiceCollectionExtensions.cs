using Asp.Versioning;
using Engage360Suite.Application.Interfaces;
using Engage360Suite.Infrastructure.Configuration;
using Engage360Suite.Infrastructure.Filters;
using Engage360Suite.Infrastructure.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Polly;
using Polly.Extensions.Http;
using Swashbuckle.AspNetCore.SwaggerGen;

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
            // Framework infra
            services.AddHealthChecks();
            services.AddVersioningAndSwagger();
            services.AddApplicationServices();
            services.AddInfrastructureServices(config);

            // Forwarded-headers options
            services.Configure<ForwardedHeadersOptions>(opts =>
            {
                opts.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor |
                    ForwardedHeaders.XForwardedProto;
                opts.KnownNetworks.Clear();
                opts.KnownProxies.Clear();
            });
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
            services.AddProblemDetails();
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
            // ──────────────────────────────────────────────────────
            // 1. Observability – register FIRST so all later components
            //    (MVC, HttpClient, custom meters) are auto-instrumented
            // ──────────────────────────────────────────────────────
            services.AddOpenTelemetry()
                    .ConfigureResource(r => r.AddService(serviceName: "Engage360Suite.API"))
                    .WithMetrics(builder =>
                    {
                        builder.AddMeter("Engage360Suite.Infrastructure")   // queue metrics
                               .AddAspNetCoreInstrumentation()              // request metrics
                               .AddHttpClientInstrumentation()              // outbound calls
                               .AddPrometheusExporter();                    // /metrics endpoint
                    });

            // ──────────────────────────────────────────────────────
            // 2. Options binding + validation (fail-fast)
            //    – delegated to helper extension methods
            // ──────────────────────────────────────────────────────
            services.AddPingerbotOptions(configuration)
                    .AddServiceBusOptions(configuration)
                    .AddLeadQueueOptions(configuration)
                    .AddSingleton<IValidateOptions<PingerbotOptions>, PingerbotOptionsValidator>()
                    .AddSingleton<IValidateOptions<ServiceBusOptions>, ServiceBusOptionsValidator>()
                    .AddSingleton<IValidateOptions<LeadQueueOptions>, LeadQueueOptionsValidator>();

            // ──────────────────────────────────────────────────────
            // 3. Conditional queue (exactly one singleton)
            // ──────────────────────────────────────────────────────
            services.AddSingleton<ServiceBusLeadQueue>();
            services.AddSingleton<InMemoryLeadQueue>();

            services.AddSingleton<ILeadQueue>(sp =>
                configuration.GetValue<bool>("ServiceBus:Enabled")
                            ? sp.GetRequiredService<ServiceBusLeadQueue>()
                            : sp.GetRequiredService<InMemoryLeadQueue>());

            // ──────────────────────────────────────────────────────
            // 4. Typed HttpClient with shared Polly resiliency policies
            // ──────────────────────────────────────────────────────
            var retryPolicy = GetRetryPolicy();
            var circuitBreaker = GetCircuitBreakerPolicy();

            services.AddHttpClient<IWhatsAppService, WhatsAppService>((sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<PingerbotOptions>>().Value;
                client.BaseAddress = new Uri(opts.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(10);
            })
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(circuitBreaker);

            // ──────────────────────────────────────────────────────
            // 5. Background worker
            // ──────────────────────────────────────────────────────
            services.AddHostedService<LeadProcessingService>();
            
            return services;
        }

        // ──────────────────────────────────────────────────────────
        //  Polly helpers – allocate once per AppDomain
        // ──────────────────────────────────────────────────────────
        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
            HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))); // 2s,4s,8s

        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy() =>
            HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
    }
}
