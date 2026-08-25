namespace HelljonVibe.Identity.Api.Configuration;

/// <summary>
/// JWT configuration settings.
/// </summary>
public class JwtSettings {
    /// <summary>
    /// Gets or sets the JWT secret key.
    /// </summary>
    public string? Secret { get; set; }

    /// <summary>
    /// Gets or sets the JWT issuer.
    /// </summary>
    public string? Issuer { get; set; }

    /// <summary>
    /// Gets or sets the JWT audience.
    /// </summary>
    public string? Audience { get; set; }

    /// <summary>
    /// Gets or sets the access token expiration time in minutes.
    /// </summary>
    public int ExpiryInMinutes { get; set; } = 15;

    /// <summary>
    /// Gets or sets the refresh token expiration time in days.
    /// </summary>
    public int RefreshExpiryInDays { get; set; } = 7;
}