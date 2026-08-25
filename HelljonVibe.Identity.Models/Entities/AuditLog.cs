using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HelljonVibe.Identity.Models.Entities;

/// <summary>
/// Represents an audit log entry for tracking user actions and system events.
/// </summary>
public class AuditLog {
    /// <summary>
    /// Gets or sets the unique identifier for the audit log entry.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the user identifier who performed the action, or null for system events.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Gets or sets the action that was performed.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of entity affected by the action.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the entity affected by the action.
    /// </summary>
    [MaxLength(100)]
    public string? EntityId { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the audit log entry.
    /// </summary>
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the IP address from which the action was performed.
    /// </summary>
    [MaxLength(45)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// Gets or sets the user agent of the client that performed the action.
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Gets or sets the encrypted details of the action.
    /// Contains JSON data with additional context.
    /// </summary>
    [MaxLength(4000)]
    public string? EncryptedDetails { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the action was successful.
    /// </summary>
    [Required]
    public bool IsSuccess { get; set; } = true;

    /// <summary>
    /// Gets or sets the error message if the action failed.
    /// </summary>
    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the duration of the action in milliseconds.
    /// </summary>
    public long? DurationMs { get; set; }

    // Navigation properties
    /// <summary>
    /// Gets or sets the user who performed the action.
    /// </summary>
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }

    /// <summary>
    /// Gets the decrypted details.
    /// Note: This is a transient property and not stored in the database.
    /// </summary>
    [NotMapped]
    public string Details => string.Empty; // Will be decrypted by service layer
}
