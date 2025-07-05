using Engage360Suite.Presentation;
using Engage360Suite.Presentation.Extensions;
using Serilog;

Log.Logger = LoggingBootstrap.CreateLogger();
try
{
    Log.Logger = LoggingBootstrap.CreateLogger();
}
catch (Exception bootstrapEx)
{
    Console.Error.WriteLine($"Fatal error creating bootstrap logger: {bootstrapEx}");
    return;
}

try
{
    Log.Information("Starting host");

    var builder = WebApplication.CreateBuilder(args).ConfigureAppSettings();
    builder.Host.UseObservability();
    builder.Services.AddDefaultServices(builder.Configuration);

    var app = builder.Build();
    app.UseApiPipeline(builder.Configuration);
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
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