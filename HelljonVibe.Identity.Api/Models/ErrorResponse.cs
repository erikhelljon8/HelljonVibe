namespace HelljonVibe.Identity.Api.Models;

/// <summary>
/// Represents an error response.
/// </summary>
public class ErrorResponse {
    /// <summary>
    /// Gets or sets the HTTP status code.
    /// </summary>
    public int StatusCode { get; set; } = 500;

    /// <summary>
    /// Gets or sets the error code.
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the error details.
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Gets or sets the stack trace (only in development).
    /// </summary>
    public string? StackTrace { get; set; }

    /// <summary>
    /// Gets or sets the validation errors.
    /// </summary>
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
}

/// <summary>
/// Represents a validation error response.
/// </summary>
public class ValidationErrorResponse : ErrorResponse {
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationErrorResponse"/> class.
    /// </summary>
    public ValidationErrorResponse() {
        StatusCode = 400;
        ErrorCode = "VALIDATION_ERROR";
        Message = "One or more validation errors occurred";
        ValidationErrors = new Dictionary<string, string[]>();
    }
}

/// <summary>
/// Represents a successful response with a message.
/// </summary>
public class SuccessResponse {
    /// <summary>
    /// Gets or sets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; set; } = true;

    /// <summary>
    /// Gets or sets the success message.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the data.
    /// </summary>
    public object? Data { get; set; }
}

/// <summary>
/// Represents a paged response.
/// </summary>
/// <typeparam name="T">The type of data.</typeparam>
public class PagedResponse<T> {

    public IReadOnlyList<T>? Data { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}