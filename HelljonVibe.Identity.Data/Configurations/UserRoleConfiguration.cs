using HelljonVibe.Identity.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace HelljonVibe.Identity.Data.Configurations;

/// <summary>
/// Entity configuration for the UserRole entity.
/// </summary>
public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole> {
    /// <summary>
    /// Configures the UserRole entity.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<UserRole> builder) {
        // Table name
        builder.ToTable("UserRoles");

        // Composite primary key
        builder.HasKey(ur => new { ur.UserId, ur.RoleId });

        // Required fields
        builder.Property(ur => ur.UserId)
            .IsRequired();

        builder.Property(ur => ur.RoleId)
            .IsRequired();

        // Default values
        builder.Property(ur => ur.AssignedDate)
            .HasDefaultValueSql("GETUTCDATE()");

        // Navigation properties
        builder.HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ur => ur.AssignedByUser)
            .WithMany() // no inverse provided because AssignedUserClients is a different entity type
            .HasForeignKey(ur => ur.AssignedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
