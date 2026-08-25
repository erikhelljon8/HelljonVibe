using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HelljonVibe.Identity.Models.Entities;

/// <summary>
/// Represents a role in the Identity system.
/// </summary>
public class Role {
    /// <summary>
    /// Gets or sets the unique identifier for the role.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the role. Must be unique.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the role.
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is a system role.
    /// System roles cannot be deleted or modified.
    /// </summary>
    [Required]
    public bool IsSystemRole { get; set; } = false;

    /// <summary>
    /// Gets or sets the date and time when the role was created.
    /// </summary>
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the role was last modified.
    /// </summary>
    public DateTimeOffset? ModifiedDate { get; set; }

    // Navigation properties
    /// <summary>
    /// Gets or sets the collection of user roles.
    /// </summary>
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}