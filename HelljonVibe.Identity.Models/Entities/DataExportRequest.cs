using HelljonVibe.Identity.Models.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HelljonVibe.Identity.Models.Entities;

/// <summary>
/// Represents a GDPR data export request.
/// Users can request an export of their personal data in a machine-readable format.
/// </summary>
public class DataExportRequest {
    /// <summary>
    /// Gets or sets the unique identifier for the data export request.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the user identifier who requested the data export.
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the status of the data export request.
    /// Values: Pending, Processing, Completed, Expired, Failed
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = GdprConstants.DataExportStatusPending;

    /// <summary>
    /// Gets or sets the comma-separated list of data categories to export.
    /// If empty, exports all data.
    /// </summary>
    [MaxLength(500)]
    public string? DataCategories { get; set; }

    /// <summary>
    /// Gets or sets the format of the exported data (e.g., "json", "csv", "xml").
    /// </summary>
    [MaxLength(20)]
    public string ExportFormat { get; set; } = "json";

    /// <summary>
    /// Gets or sets the encrypted file path where the exported data is stored.
    /// </summary>
    [MaxLength(512)]
    public string? EncryptedFilePath { get; set; }

    /// <summary>
    /// Gets or sets the download token for accessing the exported data.
    /// </summary>
    [MaxLength(255)]
    public string? EncryptedDownloadToken { get; set; }

    /// <summary>
    /// Gets or sets the hashed download token for verification.
    /// </summary>
    [MaxLength(255)]
    public string? DownloadTokenHash { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the request was created.
    /// </summary>
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the request was completed.
    /// </summary>
    public DateTimeOffset? CompletedDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the request expires.
    /// </summary>
    public DateTimeOffset? ExpiresDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the exported data was downloaded.
    /// </summary>
    public DateTimeOffset? DownloadedDate { get; set; }

    /// <summary>
    /// Gets or sets the number of times the exported data has been downloaded.
    /// </summary>
    [Required]
    public int DownloadCount { get; set; } = 0;

    /// <summary>
    /// Gets or sets the size of the exported data in bytes.
    /// </summary>
    public long? FileSizeBytes { get; set; }

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

    // Navigation properties
    /// <summary>
    /// Gets or sets the user.
    /// </summary>
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Gets the decrypted file path.
    /// Note: This is a transient property and not stored in the database.
    /// </summary>
    [NotMapped]
    public string FilePath => string.Empty; // Will be decrypted by service layer

    /// <summary>
    /// Gets the decrypted download token.
    /// Note: This is a transient property and not stored in the database.
    /// </summary>
    [NotMapped]
    public string DownloadToken => string.Empty; // Will be decrypted by service layer
}