using HelljonVibe.Identity.Api.Models;
using System.Net;
using System.Text.Json;

namespace HelljonVibe.Identity.Api.Middleware;

/// <summary>
/// Middleware for handling exceptions globally.
/// </summary>
public class ExceptionHandlingMiddleware {
    /// <summary>
    /// The next middleware in the pipeline.
    /// </summary>
    private readonly RequestDelegate _next;

    /// <summary>
    /// The logger.
    /// </summary>
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger.</param>
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger) {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public async Task InvokeAsync(HttpContext context) {
        try {
            await _next(context);
        } catch (Exception ex) {
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Handles an exception.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="exception">The exception.</param>
    private async Task HandleExceptionAsync(HttpContext context, Exception exception) {
        _logger.LogError(exception, "An unhandled exception occurred");

        var response = new ErrorResponse {
            Message = "An error occurred while processing your request",
            ErrorCode = "INTERNAL_SERVER_ERROR",
            Details = exception.Message
        };

        // Handle specific exception types
        switch (exception) {
            case UnauthorizedAccessException:
            response.StatusCode = (int)HttpStatusCode.Unauthorized;
            response.ErrorCode = "UNAUTHORIZED";
            response.Message = "You are not authorized to access this resource";
            break;

            case ArgumentException:
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            response.ErrorCode = "BAD_REQUEST";
            response.Message = exception.Message;
            break;

            case ArgumentNullException:
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            response.ErrorCode = "BAD_REQUEST";
            response.Message = exception.Message;
            break;

            case InvalidOperationException:
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            response.ErrorCode = "INVALID_OPERATION";
            response.Message = exception.Message;
            break;

            case KeyNotFoundException:
            response.StatusCode = (int)HttpStatusCode.NotFound;
            response.ErrorCode = "NOT_FOUND";
            response.Message = exception.Message;
            break;

            default:
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            response.ErrorCode = "INTERNAL_SERVER_ERROR";
            response.Message = "An unexpected error occurred";
            break;
        }

        // In development, include stack trace
        if (context.RequestServices.GetService(typeof(Microsoft.AspNetCore.Hosting.IWebHostEnvironment)) is Microsoft.AspNetCore.Hosting.IWebHostEnvironment env && env.EnvironmentName == "Development") {
            response.StackTrace = exception.StackTrace;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = response.StatusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        }));
    }
}

/// <summary>
/// Extension methods for the ExceptionHandlingMiddleware.
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions {
    /// <summary>
    /// Adds the exception handling middleware to the pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder.</returns>
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app) {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}