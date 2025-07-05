using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Engage360Suite.Presentation.Middlewares
{
    /// <summary>
    /// Catches all un-handled exceptions, logs them once, and returns an
    /// RFC 9457 <c>application/problem+json</c> response.
    /// </summary>
    public sealed class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly ProblemDetailsFactory _problemFactory;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger,
            ProblemDetailsFactory problemFactory)
        {
            _next = next;
            _logger = logger;
            _problemFactory = problemFactory;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unhandled exception while processing {Method} {Path}",
                    httpContext.Request.Method,
                    httpContext.Request.Path);

                if (httpContext.Response.HasStarted)
                    throw; // too late to write problem-details

                var status = ex switch
                {
                    ValidationException => StatusCodes.Status400BadRequest,
                    KeyNotFoundException => StatusCodes.Status404NotFound,
                    UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                    _ => StatusCodes.Status500InternalServerError
                };

                var pd = _problemFactory.CreateProblemDetails(
                    httpContext,
                    statusCode: status,
                    detail: ex.Message);

                pd.Extensions["traceId"] =
                    Activity.Current?.Id ?? httpContext.TraceIdentifier;

                httpContext.Response.Clear();
                httpContext.Response.StatusCode = pd.Status!.Value;
                httpContext.Response.ContentType = "application/problem+json";
                await httpContext.Response.WriteAsJsonAsync(pd);
            }
        }
    }
}
