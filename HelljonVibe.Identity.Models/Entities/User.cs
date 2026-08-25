using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HelljonVibe.Identity.Models.Entities;

/// <summary>
/// Represents a user in the Identity system.
/// All personally identifiable information (PII) is encrypted at rest.
/// </summary>
public class User {
    /// <summary>
    /// Gets or sets the unique identifier for the user.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the encrypted email address.
    /// </summary>
    [Required]
    [MaxLength(256)]
    public string EncryptedEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the encrypted username. Must be unique.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string EncryptedUsername { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the hashed password using BCrypt.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password salt.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string PasswordSalt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the email has been confirmed.
    /// </summary>
    [Required]
    public bool EmailConfirmed { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether two-factor authentication is enabled.
    /// </summary>
    [Required]
    public bool TwoFactorEnabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the date and time when the user was locked out, or null if not locked out.
    /// </summary>
    public DateTimeOffset? LockoutEndDateUtc { get; set; }

    /// <summary>
    /// Gets or sets the number of failed login attempts.
    /// </summary>
    [Required]
    public int AccessFailedCount { get; set; } = 0;

    /// <summary>
    /// Gets or sets the date and time when the user was created.
    /// </summary>
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the date and time of the last login, or null if never logged in.
    /// </summary>
    public DateTimeOffset? LastLoginDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user account is approved.
    /// Only approved users can access the application.
    /// </summary>
    [Required]
    public bool IsApproved { get; set; } = false;

    /// <summary>
    /// Gets or sets the user-specific encryption key (encrypted with master key).
    /// Used for encrypting user-specific data.
    /// </summary>
    [MaxLength(512)]
    public string? EncryptionKey { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user account is active.
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;

    // GDPR-specific fields
    /// <summary>
    /// Gets or sets the date and time when the user accepted the privacy policy.
    /// </summary>
    public DateTimeOffset? PrivacyPolicyAcceptedDate { get; set; }

    /// <summary>
    /// Gets or sets the version of the privacy policy that the user accepted.
    /// </summary>
    [MaxLength(50)]
    public string? PrivacyPolicyVersion { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the user last updated their consent preferences.
    /// </summary>
    public DateTimeOffset? ConsentLastUpdatedDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user has accepted the terms of service.
    /// </summary>
    [Required]
    public bool TermsAccepted { get; set; } = false;

    /// <summary>
    /// Gets or sets the date and time when the user accepted the terms of service.
    /// </summary>
    public DateTimeOffset? TermsAcceptedDate { get; set; }

    /// <summary>
    /// Gets or sets the version of the terms of service that the user accepted.
    /// </summary>
    [MaxLength(50)]
    public string? TermsVersion { get; set; }

    // Navigation properties
    /// <summary>
    /// Gets or sets the collection of user roles.
    /// </summary>
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    /// <summary>
    /// Gets or sets the collection of user tokens (email confirmation, password reset, etc.).
    /// </summary>
    public virtual ICollection<UserToken> Tokens { get; set; } = new List<UserToken>();

    /// <summary>
    /// Gets or sets the collection of two-factor backup codes.
    /// </summary>
    public virtual ICollection<TwoFactorBackupCode> BackupCodes { get; set; } = new List<TwoFactorBackupCode>();

    /// <summary>
    /// Gets or sets the collection of user-client connections.
    /// </summary>
    public virtual ICollection<UserClient> UserClients { get; set; } = new List<UserClient>();

    /// <summary>
    /// Gets or sets the collection of audit logs for this user.
    /// </summary>
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    /// <summary>
    /// Gets or sets the collection of user-client connections assigned by this user.
    /// </summary>
    [InverseProperty("AssignedByUser")]
    public virtual ICollection<UserClient> AssignedUserClients { get; set; } = new List<UserClient>();

    /// <summary>
    /// Gets or sets the collection of user consents.
    /// </summary>
    public virtual ICollection<UserConsent> Consents { get; set; } = new List<UserConsent>();

    /// <summary>
    /// Gets or sets the collection of data export requests.
    /// </summary>
    public virtual ICollection<DataExportRequest> DataExportRequests { get; set; } = new List<DataExportRequest>();

    /// <summary>
    /// Gets or sets the collection of account deletion requests.
    /// </summary>
    public virtual ICollection<AccountDeletionRequest> AccountDeletionRequests { get; set; } = new List<AccountDeletionRequest>();

    /// <summary>
    /// Gets the decrypted email address.
    /// Note: This is a transient property and not stored in the database.
    /// </summary>
    [NotMapped]
    public string Email => string.Empty; // Will be decrypted by service layer

    /// <summary>
    /// Gets the decrypted username.
    /// Note: This is a transient property and not stored in the database.
    /// </summary>
    [NotMapped]
    public string Username => string.Empty; // Will be decrypted by service layer
}