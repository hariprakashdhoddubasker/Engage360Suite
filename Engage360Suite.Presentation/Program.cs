using Engage360Suite.Presentation.Extensions;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();
try
{
    Log.Information("Starting host");

    var builder = WebApplication.CreateBuilder(args);

    // --------------------Configure logging & configuration --------------------
    builder.Host
    .UseSerilog(
        (ctx, services, cfg) => cfg
            .ReadFrom.Configuration(ctx.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());

    builder.Configuration
       .AddJsonFile("serilog.json", optional: true, reloadOnChange: true)
       .AddEnvironmentVariables();

    // --------------------DI registrations --------------------
    builder.Services.AddHealthChecks();
    builder.Services.AddVersioningAndSwagger()
                    .AddApplicationServices()
                    .AddInfrastructureServices(builder.Configuration);

    // Forwarded-headers so the app knows the real client IP / scheme
    builder.Services.Configure<ForwardedHeadersOptions>(o =>
    {
        o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        o.KnownNetworks.Clear();
        o.KnownProxies.Clear();
    });

    var app = builder.Build();

    // --------------------HTTP pipeline --------------------
    app.UseForwardedHeaders();
    app.UseSwaggerAccordingToEnvironment()
       .UseStaticAndRouting()
       .MapAppEndpoints();

    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}


/// <summary>
/// Enables integration/functional tests to spin-up the host without
/// touching the top-level statements.
/// </summary>
public static partial class Program
{
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.Configure(app =>
                {
                    throw new NotImplementedException(
                        "With the minimal-hosting model Startup isn't used; " +
                        "tests should call WebApplicationFactory<Program>.");
                });
            });
}