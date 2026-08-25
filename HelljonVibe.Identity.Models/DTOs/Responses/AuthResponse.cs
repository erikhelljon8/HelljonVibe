using System;
using System.Collections.Generic;
using System.Text;

namespace HelljonVibe.Identity.Models.DTOs.Responses;

/// <summary>
/// DTO for authentication responses.
/// </summary>
public class AuthResponse {
    /// <summary>
    /// Gets or sets a value indicating whether the authentication was successful.
    /// </summary>
    public bool IsSuccess { get; set; } = true;

    /// <summary>
    /// Gets or sets the access token (JWT).
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// Gets or sets the refresh token.
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Gets or sets the access token expiration in seconds.
    /// </summary>
    public long ExpiresIn { get; set; }

    /// <summary>
    /// Gets or sets the token type (e.g., "Bearer").
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the email is verified.
    /// </summary>
    public bool EmailVerified { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether two-factor authentication is enabled.
    /// </summary>
    public bool TwoFactorEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the account is approved.
    /// </summary>
    public bool IsApproved { get; set; }

    /// <summary>
    /// Gets or sets the roles assigned to the user.
    /// </summary>
    public IList<string> Roles { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets a value indicating whether the user must change their password.
    /// </summary>
    public bool RequiresPasswordChange { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether two-factor authentication is required for this session.
    /// </summary>
    public bool RequiresTwoFactor { get; set; }

    /// <summary>
    /// Gets or sets the two-factor authentication setup URL (for TOTP).
    /// </summary>
    public string? TwoFactorSetupUrl { get; set; }

    /// <summary>
    /// Gets or sets the two-factor authentication secret (for TOTP).
    /// </summary>
    public string? TwoFactorSecret { get; set; }

    /// <summary>
    /// Gets or sets the backup codes for two-factor authentication.
    /// </summary>
    public IList<string> TwoFactorBackupCodes { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the error message if authentication failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the error code if authentication failed.
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets the number of failed login attempts.
    /// </summary>
    public int FailedAttempts { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the account is locked out.
    /// </summary>
    public bool IsLockedOut { get; set; }
}