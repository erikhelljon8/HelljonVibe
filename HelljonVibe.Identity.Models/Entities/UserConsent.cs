using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HelljonVibe.Identity.Models.Entities;

/// <summary>
/// Represents user consent for various data processing purposes.
/// Required for GDPR compliance to track user consent for different processing activities.
/// </summary>
public class UserConsent {
    /// <summary>
    /// Gets or sets the unique identifier for the consent record.
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
    /// Gets or sets the type of consent (e.g., "PrivacyPolicy", "Marketing", "Analytics").
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string ConsentType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version of the consent document.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string ConsentVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the user has given consent.
    /// </summary>
    [Required]
    public bool IsConsentGiven { get; set; } = false;

    /// <summary>
    /// Gets or sets the date and time when consent was given.
    /// </summary>
    public DateTimeOffset? ConsentGivenDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when consent was withdrawn.
    /// </summary>
    public DateTimeOffset? ConsentWithdrawnDate { get; set; }

    /// <summary>
    /// Gets or sets the IP address from which consent was given.
    /// </summary>
    [MaxLength(45)]
    public string? ConsentIpAddress { get; set; }

    /// <summary>
    /// Gets or sets the user agent of the client that gave consent.
    /// </summary>
    [MaxLength(500)]
    public string? ConsentUserAgent { get; set; }

    /// <summary>
    /// Gets or sets the encrypted consent document text.
    /// Stores the actual text of the consent document at the time of acceptance.
    /// </summary>
    [MaxLength(4000)]
    public string? EncryptedConsentDocument { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the consent record was created.
    /// </summary>
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the consent record was last modified.
    /// </summary>
    public DateTimeOffset? ModifiedDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this consent is required for the user to use the service.
    /// </summary>
    [Required]
    public bool IsRequired { get; set; } = false;

    // Navigation properties
    /// <summary>
    /// Gets or sets the user.
    /// </summary>
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Gets the decrypted consent document text.
    /// Note: This is a transient property and not stored in the database.
    /// </summary>
    [NotMapped]
    public string ConsentDocument => string.Empty; // Will be decrypted by service layer
}