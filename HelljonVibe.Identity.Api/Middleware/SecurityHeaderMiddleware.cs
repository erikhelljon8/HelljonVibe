namespace HelljonVibe.Identity.Api.Middleware;


/// <summary>
/// Middleware for adding security headers to HTTP responses.
/// </summary>
public class SecurityHeadersMiddleware {
    /// <summary>
    /// The next middleware in the pipeline.
    /// </summary>
    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityHeadersMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    public SecurityHeadersMiddleware(RequestDelegate next) {
        _next = next;
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public async Task InvokeAsync(HttpContext context) {
        // Add security headers
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-Xss-Protection", "1; mode=block");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
        context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self'; connect-src 'self'; frame-src 'none'; object-src 'none'; base-uri 'self'; form-action 'self'");

        // HSTS header (only for HTTPS)
        if (context.Request.IsHttps) {
            context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
        }

        // Cache control for sensitive pages
        if (context.Request.Path.StartsWithSegments("/api")) {
            context.Response.Headers.Append("Cache-Control", "no-store, no-cache, must-revalidate, private");
            context.Response.Headers.Append("Pragma", "no-cache");
            context.Response.Headers.Append("Expires", "0");
        }

        await _next(context);
    }
}

/// <summary>
/// Extension methods for the SecurityHeadersMiddleware.
/// </summary>
public static class SecurityHeadersMiddlewareExtensions {
    /// <summary>
    /// Adds the security headers middleware to the pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder.</returns>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app) {
        return app.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
