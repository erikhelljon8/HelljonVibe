namespace HelljonVibe.Identity.Api.Configuration;

/// <summary>
/// Application configuration settings.
/// </summary>
public class AppSettings {
    /// <summary>
    /// Gets or sets the base URL of the API.
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// Gets or sets the frontend URL for CORS and email links.
    /// </summary>
    public string? FrontendUrl { get; set; }

    /// <summary>
    /// Gets or sets the environment name.
    /// </summary>
    public string? Environment { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the application is in development mode.
    /// </summary>
    public bool IsDevelopment { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of failed login attempts before lockout.
    /// </summary>
    public int MaxFailedAccessAttempts { get; set; } = 5;

    /// <summary>
    /// Gets or sets the lockout duration in minutes.
    /// </summary>
    public int LockoutDurationInMinutes { get; set; } = 15;

    /// <summary>
    /// Gets or sets the password minimum length.
    /// </summary>
    public int PasswordMinLength { get; set; } = 12;

    /// <summary>
    /// Gets or sets the password maximum length.
    /// </summary>
    public int PasswordMaxLength { get; set; } = 128;

    /// <summary>
    /// Gets or sets a value indicating whether client-side encryption is required.
    /// </summary>
    public bool RequireClientSideEncryption { get; set; } = false;

    /// <summary>
    /// Gets or sets the token cleanup interval in hours.
    /// </summary>
    public int TokenCleanupIntervalInHours { get; set; } = 24;

    /// <summary>
    /// Gets or sets the audit log retention in days.
    /// </summary>
    public int AuditLogRetentionInDays { get; set; } = 365 * 6; // 6 years for GDPR
}
