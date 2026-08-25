using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HelljonVibe.Identity.Models.Entities;

/// <summary>
/// Represents the many-to-many relationship between users and clients.
/// Defines which users have access to which clients.
/// </summary>
public class UserClient {
    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the client identifier.
    /// </summary>
    [Required]
    public Guid ClientId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the user was assigned to the client.
    /// </summary>
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTimeOffset AssignedDate { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the user who assigned this connection.
    /// </summary>
    [Required]
    public Guid AssignedByUserId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this connection is active.
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;

    // Navigation properties
    /// <summary>
    /// Gets or sets the user.
    /// </summary>
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Gets or sets the client.
    /// </summary>
    [ForeignKey("ClientId")]
    public virtual Client Client { get; set; } = null!;

    /// <summary>
    /// Gets or sets the user who assigned this connection.
    /// </summary>
    [ForeignKey("AssignedByUserId")]
    public virtual User AssignedByUser { get; set; } = null!;
}