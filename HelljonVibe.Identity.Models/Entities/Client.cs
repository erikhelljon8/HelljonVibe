using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HelljonVibe.Identity.Models.Entities;

/// <summary>
/// Represents a client application that can integrate with the Identity system.
/// </summary>
public class Client {
    /// <summary>
    /// Gets or sets the unique identifier for the client.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the client.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique client identifier.
    /// Used for OAuth/OIDC flows.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the encrypted client secret.
    /// </summary>
    [Required]
    [MaxLength(512)]
    public string EncryptedClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the client is active.
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the date and time when the client was created.
    /// </summary>
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the description of the client.
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the redirect URIs for OAuth flows.
    /// Stored as JSON array.
    /// </summary>
    [MaxLength(2000)]
    public string? RedirectUris { get; set; }

    /// <summary>
    /// Gets or sets the logout redirect URIs.
    /// Stored as JSON array.
    /// </summary>
    [MaxLength(2000)]
    public string? PostLogoutRedirectUris { get; set; }

    /// <summary>
    /// Gets or sets the allowed CORS origins.
    /// Stored as JSON array.
    /// </summary>
    [MaxLength(2000)]
    public string? AllowedCorsOrigins { get; set; }

    /// <summary>
    /// Gets or sets the client type (e.g., "web", "spa", "mobile", "desktop").
    /// </summary>
    [MaxLength(50)]
    public string? ClientType { get; set; }

    // Navigation properties
    /// <summary>
    /// Gets or sets the collection of user-client connections.
    /// </summary>
    public virtual ICollection<UserClient> UserClients { get; set; } = new List<UserClient>();

    /// <summary>
    /// Gets the decrypted client secret.
    /// Note: This is a transient property and not stored in the database.
    /// </summary>
    [NotMapped]
    public string ClientSecret => string.Empty; // Will be decrypted by service layer
}