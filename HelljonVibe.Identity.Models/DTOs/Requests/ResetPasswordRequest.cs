using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HelljonVibe.Identity.Models.DTOs.Requests;

/// <summary>
/// DTO for password reset requests.
/// </summary>
public class ResetPasswordRequest {
    /// <summary>
    /// Gets or sets the password reset token.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the new password.
    /// </summary>
    [Required]
    [MinLength(12)]
    [MaxLength(128)]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the new password confirmation.
    /// </summary>
    [Required]
    [Compare("NewPassword")]
    public string ConfirmNewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the encrypted new password (client-side encrypted).
    /// This is optional and used when client-side encryption is enabled.
    /// </summary>
    [MaxLength(512)]
    public string? EncryptedNewPassword { get; set; }
}