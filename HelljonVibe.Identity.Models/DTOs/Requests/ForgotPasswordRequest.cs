using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HelljonVibe.Identity.Models.DTOs.Requests;

/// <summary>
/// DTO for forgot password requests.
/// </summary>
public class ForgotPasswordRequest {
    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the client ID for which the password reset is requested.
    /// </summary>
    [MaxLength(100)]
    public string? ClientId { get; set; }

    /// <summary>
    /// Gets or sets the recaptcha token for bot protection.
    /// </summary>
    [MaxLength(512)]
    public string? RecaptchaToken { get; set; }
}
