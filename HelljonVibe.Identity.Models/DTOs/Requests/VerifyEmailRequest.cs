using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HelljonVibe.Identity.Models.DTOs.Requests;

/// <summary>
/// DTO for email verification requests.
/// </summary>
public class VerifyEmailRequest {
    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the email verification token.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Token { get; set; } = string.Empty;
}