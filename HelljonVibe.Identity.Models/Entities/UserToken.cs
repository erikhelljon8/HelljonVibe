using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HelljonVibe.Identity.Models.Entities;

/// <summary>
/// Represents a token issued to a user for various purposes (email confirmation, password reset, etc.).
/// </summary>
public class UserToken {
    /// <summary>
    /// Gets or sets the unique identifier for the token.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the type of token (e.g., "EmailConfirmation", "PasswordReset", "RefreshToken").
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string TokenType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the encrypted token value.
    /// </summary>
    [Required]
    [MaxLength(512)]
    public string EncryptedToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the hashed token value for verification.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when the token expires.
    /// </summary>
    [Required]
    public DateTimeOffset Expires { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the token has been used.
    /// </summary>
    [Required]
    public bool IsUsed { get; set; } = false;

    /// <summary>
    /// Gets or sets the date and time when the token was used.
    /// </summary>
    public DateTimeOffset? UsedDate { get; set; }

    /// <summary>
    /// Gets or sets the IP address from which the token was requested.
    /// </summary>
    [MaxLength(45)]
    public string? RequestIpAddress { get; set; }

    /// <summary>
    /// Gets or sets the user agent of the client that requested the token.
    /// </summary>
    [MaxLength(500)]
    public string? RequestUserAgent { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the token was created.
    /// </summary>
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    /// <summary>
    /// Gets or sets the user.
    /// </summary>
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Gets the decrypted token value.
    /// Note: This is a transient property and not stored in the database.
    /// </summary>
    [NotMapped]
    public string Token => string.Empty; // Will be decrypted by service layer
}