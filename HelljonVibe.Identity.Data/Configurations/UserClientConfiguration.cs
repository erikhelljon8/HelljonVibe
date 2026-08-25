using HelljonVibe.Identity.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace HelljonVibe.Identity.Data.Configurations;

/// <summary>
/// Entity configuration for the UserClient entity.
/// </summary>
public class UserClientConfiguration : IEntityTypeConfiguration<UserClient> {
    /// <summary>
    /// Configures the UserClient entity.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<UserClient> builder) {
        // Table name
        builder.ToTable("UserClients");

        // Composite primary key
        builder.HasKey(uc => new { uc.UserId, uc.ClientId });

        // Required fields
        builder.Property(uc => uc.UserId)
            .IsRequired();

        builder.Property(uc => uc.ClientId)
            .IsRequired();

        builder.Property(uc => uc.AssignedByUserId)
            .IsRequired();

        // Default values
        builder.Property(uc => uc.AssignedDate)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(uc => uc.IsActive)
            .HasDefaultValue(true);

        // Navigation properties
        builder.HasOne(uc => uc.User)
            .WithMany(u => u.UserClients)
            .HasForeignKey(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(uc => uc.Client)
            .WithMany(c => c.UserClients)
            .HasForeignKey(uc => uc.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(uc => uc.AssignedByUser)
            .WithMany(u => u.AssignedUserClients)
            .HasForeignKey(uc => uc.AssignedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}