using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HelljonVibe.Identity.Models.DTOs.Requests;

/// <summary>
/// DTO for user registration requests.
/// </summary>
public class RegisterRequest {
    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the username. Must be unique.
    /// </summary>
    [Required]
    [MinLength(3)]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    [Required]
    [MinLength(12)]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password confirmation.
    /// </summary>
    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the user accepts the terms of service.
    /// </summary>
    [Required]
    public bool AcceptTerms { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the user accepts the privacy policy.
    /// </summary>
    [Required]
    public bool AcceptPrivacyPolicy { get; set; } = false;

    /// <summary>
    /// Gets or sets the version of the terms of service that the user accepts.
    /// </summary>
    [MaxLength(50)]
    public string? TermsVersion { get; set; }

    /// <summary>
    /// Gets or sets the version of the privacy policy that the user accepts.
    /// </summary>
    [MaxLength(50)]
    public string? PrivacyPolicyVersion { get; set; }

    /// <summary>
    /// Gets or sets the client ID if registering through a specific client.
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
    /// Gets or sets the public key used for client-side encryption.
    /// Used to verify the encryption.
    /// </summary>
    [MaxLength(512)]
    public string? PublicKey { get; set; }
}