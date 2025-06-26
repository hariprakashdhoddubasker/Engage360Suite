using Serilog;
using ILogger = Serilog.ILogger;

namespace Engage360Suite.Presentation
{
    public class LoggingBootstrap
    {
        /// <summary>
        /// Creates a Serilog “bootstrap” logger so we capture events
        /// even before the host is built.
        /// </summary>
        public static ILogger CreateLogger() =>
        new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();
    }
}
