using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HelljonVibe.Identity.Models.Entities;

/// <summary>
/// Represents the many-to-many relationship between users and roles.
/// </summary>
public class UserRole {
    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the role identifier.
    /// </summary>
    [Required]
    public Guid RoleId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the user was assigned to the role.
    /// </summary>
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTimeOffset AssignedDate { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the user who assigned this role.
    /// </summary>
    public Guid? AssignedByUserId { get; set; }

    // Navigation properties
    /// <summary>
    /// Gets or sets the user.
    /// </summary>
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Gets or sets the role.
    /// </summary>
    [ForeignKey("RoleId")]
    public virtual Role Role { get; set; } = null!;

    /// <summary>
    /// Gets or sets the user who assigned this role.
    /// </summary>
    [ForeignKey("AssignedByUserId")]
    public virtual User? AssignedByUser { get; set; }
}