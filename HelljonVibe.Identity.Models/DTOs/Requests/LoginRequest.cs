using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HelljonVibe.Identity.Models.DTOs.Requests;

/// <summary>
/// DTO for user login requests.
/// </summary>
public class LoginRequest {
    /// <summary>
    /// Gets or sets the username or email address.
    /// </summary>
    [Required]
    [MaxLength(256)]
    public string UsernameOrEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    [Required]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether to remember the login.
    /// </summary>
    public bool RememberMe { get; set; } = false;

    /// <summary>
    /// Gets or sets the two-factor authentication code.
    /// Required if the user has 2FA enabled.
    /// </summary>
    [MaxLength(20)]
    public string? TwoFactorCode { get; set; }

    /// <summary>
    /// Gets or sets the two-factor backup code.
    /// Can be used instead of the 2FA code.
    /// </summary>
    [MaxLength(20)]
    public string? TwoFactorBackupCode { get; set; }

    /// <summary>
    /// Gets or sets the client ID for which the user is logging in.
    /// </summary>
    [MaxLength(100)]
    public string? ClientId { get; set; }

    /// <summary>
    /// Gets or sets the encrypted password (client-side encrypted).
    /// This is optional and used when client-side encryption is enabled.
    /// </summary>
    [MaxLength(512)]
    public string? EncryptedPassword { get; set; }

    /// <summary>
    /// Gets or sets the device identifier for tracking.
    /// </summary>
    [MaxLength(255)]
    public string? DeviceId { get; set; }
}