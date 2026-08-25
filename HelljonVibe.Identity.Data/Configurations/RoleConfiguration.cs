using HelljonVibe.Identity.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace HelljonVibe.Identity.Data.Configurations;

/// <summary>
/// Entity configuration for the Role entity.
/// </summary>
public class RoleConfiguration : IEntityTypeConfiguration<Role> {
    /// <summary>
    /// Configures the Role entity.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Role> builder) {
        // Table name
        builder.ToTable("Roles");

        // Primary key
        builder.HasKey(r => r.Id);

        // Required fields
        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Default values
        builder.Property(r => r.IsSystemRole)
            .HasDefaultValue(false);

        builder.Property(r => r.CreatedDate)
            .HasDefaultValueSql("GETUTCDATE()");

        // Navigation properties
        builder.HasMany(r => r.UserRoles)
            .WithOne(ur => ur.Role)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}