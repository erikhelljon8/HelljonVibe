using System.Diagnostics;

namespace HelljonVibe.Identity.Api.Middleware;

/// <summary>
/// Middleware for logging HTTP requests.
/// </summary>
public class RequestLoggingMiddleware {
    /// <summary>
    /// The next middleware in the pipeline.
    /// </summary>
    private readonly RequestDelegate _next;

    /// <summary>
    /// The logger.
    /// </summary>
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestLoggingMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger.</param>
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger) {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public async Task InvokeAsync(HttpContext context) {
        var stopwatch = Stopwatch.StartNew();

        // Log request
        LogRequest(context);

        try {
            await _next(context);

            // Log response
            LogResponse(context, stopwatch.ElapsedMilliseconds);
        } catch (Exception ex) {
            // Log error
            LogError(context, ex, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    /// <summary>
    /// Logs the HTTP request.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    private void LogRequest(HttpContext context) {
        var request = context.Request;
        var user = context.User.Identity?.Name ?? "Anonymous";

        _logger.LogInformation(
            "Request: {Method} {Path} from {IpAddress} by {User} with UserAgent: {UserAgent}",
            request.Method,
            request.Path,
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            user,
            request.Headers["User-Agent"].ToString());
    }

    /// <summary>
    /// Logs the HTTP response.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="durationMs">The request duration in milliseconds.</param>
    private void LogResponse(HttpContext context, long durationMs) {
        var response = context.Response;
        var user = context.User.Identity?.Name ?? "Anonymous";

        _logger.LogInformation(
            "Response: {StatusCode} for {Method} {Path} by {User} in {Duration}ms",
            response.StatusCode,
            context.Request.Method,
            context.Request.Path,
            user,
            durationMs);
    }

    /// <summary>
    /// Logs an error.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="exception">The exception.</param>
    /// <param name="durationMs">The request duration in milliseconds.</param>
    private void LogError(HttpContext context, Exception exception, long durationMs) {
        var response = context.Response;
        var user = context.User.Identity?.Name ?? "Anonymous";

        _logger.LogError(
            exception,
            "Error: {StatusCode} for {Method} {Path} by {User} in {Duration}ms. Error: {ErrorMessage}",
            response.StatusCode,
            context.Request.Method,
            context.Request.Path,
            user,
            durationMs,
            exception.Message);
    }
}

/// <summary>
/// Extension methods for the RequestLoggingMiddleware.
/// </summary>
public static class RequestLoggingMiddlewareExtensions {
    /// <summary>
    /// Adds the request logging middleware to the pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder.</returns>
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app) {
        return app.UseMiddleware<RequestLoggingMiddleware>();
    }
}
