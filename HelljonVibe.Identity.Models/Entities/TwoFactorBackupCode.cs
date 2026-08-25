using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HelljonVibe.Identity.Models.Entities;

/// <summary>
/// Represents a backup code for two-factor authentication.
/// </summary>
public class TwoFactorBackupCode {
    /// <summary>
    /// Gets or sets the unique identifier for the backup code.
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
    /// Gets or sets the encrypted backup code.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string EncryptedCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the hashed backup code for verification.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string CodeHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the backup code has been used.
    /// </summary>
    [Required]
    public bool IsUsed { get; set; } = false;

    /// <summary>
    /// Gets or sets the date and time when the backup code was used.
    /// </summary>
    public DateTimeOffset? UsedDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the backup code was created.
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
    /// Gets the decrypted backup code.
    /// Note: This is a transient property and not stored in the database.
    /// </summary>
    [NotMapped]
    public string Code => string.Empty; // Will be decrypted by service layer
}
