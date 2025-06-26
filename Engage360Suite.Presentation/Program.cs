using Engage360Suite.Presentation;
using Engage360Suite.Presentation.Extensions;
using Serilog;

// Entry point and composition root for Engage360Suite API.
// Configures logging, services, middleware pipeline, and starts the host.

Log.Logger = LoggingBootstrap.CreateLogger();
// Bootstrap logger so we capture any failures during host build

try
{
    Log.Information("Starting host");

    // Build host and load configuration sources (JSON + env-vars)
    var builder = WebApplication.CreateBuilder(args)
                                .ConfigureAppSettings();

    // Configure structured logging via Serilog
    builder.Host.UseObservability();

    // Delegate service registrations to Startup
    var startup = new Startup(builder.Configuration);
    startup.ConfigureServices(builder.Services);

    // Build the WebApplication
    var app = builder.Build();
    startup.Configure(app);

    // Run the host asynchronously
    await app.RunAsync();
}
catch (Exception ex)
{
    // Log any host-level fatal errors and exit non-zero
    Log.Fatal(ex, "Host terminated unexpectedly");
    Environment.Exit(1);
}
finally
{
    // Ensure buffered logs are flushed
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