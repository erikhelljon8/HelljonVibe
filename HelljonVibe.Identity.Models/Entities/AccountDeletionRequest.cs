using HelljonVibe.Identity.Models.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HelljonVibe.Identity.Models.Entities;

/// <summary>
/// Represents a GDPR account deletion request (Right to be Forgotten).
/// Users can request the deletion of their account and personal data.
/// </summary>
public class AccountDeletionRequest {
    /// <summary>
    /// Gets or sets the unique identifier for the account deletion request.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the user identifier whose account is to be deleted.
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the status of the account deletion request.
    /// Values: Pending, Processing, Completed, Failed
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = GdprConstants.AccountDeletionStatusPending;

    /// <summary>
    /// Gets or sets the reason for the deletion request.
    /// </summary>
    [MaxLength(100)]
    public string DeletionReason { get; set; } = GdprConstants.DeletionReasonUserRequest;

    /// <summary>
    /// Gets or sets the user-provided reason for deletion (optional).
    /// </summary>
    [MaxLength(1000)]
    public string? UserProvidedReason { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the deletion should be immediate or after retention period.
    /// </summary>
    [Required]
    public bool ImmediateDeletion { get; set; } = false;

    /// <summary>
    /// Gets or sets the date and time when the request was created.
    /// </summary>
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the request was processed.
    /// </summary>
    public DateTimeOffset? ProcessedDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the account will be (or was) deleted.
    /// </summary>
    public DateTimeOffset? DeletionDate { get; set; }

    /// <summary>
    /// Gets or sets the user identifier who processed the deletion request (for admin-initiated deletions).
    /// </summary>
    public Guid? ProcessedByUserId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user confirmed the deletion via email.
    /// </summary>
    [Required]
    public bool EmailConfirmed { get; set; } = false;

    /// <summary>
    /// Gets or sets the encrypted confirmation token for email verification.
    /// </summary>
    [MaxLength(255)]
    public string? EncryptedConfirmationToken { get; set; }

    /// <summary>
    /// Gets or sets the hashed confirmation token for verification.
    /// </summary>
    [MaxLength(255)]
    public string? ConfirmationTokenHash { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the confirmation email was sent.
    /// </summary>
    public DateTimeOffset? ConfirmationEmailSentDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the confirmation was received.
    /// </summary>
    public DateTimeOffset? ConfirmationReceivedDate { get; set; }

    /// <summary>
    /// Gets or sets the IP address from which the request was made.
    /// </summary>
    [MaxLength(45)]
    public string? RequestIpAddress { get; set; }

    /// <summary>
    /// Gets or sets the user agent of the client that made the request.
    /// </summary>
    [MaxLength(500)]
    public string? RequestUserAgent { get; set; }

    /// <summary>
    /// Gets or sets the error message if the request failed.
    /// </summary>
    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user's personal data was anonymized instead of deleted.
    /// This may be required for legal compliance in some jurisdictions.
    /// </summary>
    [Required]
    public bool DataAnonymized { get; set; } = false;

    // Navigation properties
    /// <summary>
    /// Gets or sets the user whose account is to be deleted.
    /// </summary>
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Gets or sets the user who processed the deletion request.
    /// </summary>
    [ForeignKey("ProcessedByUserId")]
    public virtual User? ProcessedByUser { get; set; }

    /// <summary>
    /// Gets the decrypted confirmation token.
    /// Note: This is a transient property and not stored in the database.
    /// </summary>
    [NotMapped]
    public string ConfirmationToken => string.Empty; // Will be decrypted by service layer
}