using HelljonVibe.Identity.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace HelljonVibe.Identity.Data.Configurations;

/// <summary>
/// Entity configuration for the User entity.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User> {
    /// <summary>
    /// Configures the User entity.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<User> builder) {
        // Table name
        builder.ToTable("Users");

        // Primary key
        builder.HasKey(u => u.Id);

        // Required fields
        builder.Property(u => u.EncryptedEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.EncryptedUsername)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.PasswordSalt)
            .IsRequired()
            .HasMaxLength(255);

        // Default values
        builder.Property(u => u.EmailConfirmed)
            .HasDefaultValue(false);

        builder.Property(u => u.TwoFactorEnabled)
            .HasDefaultValue(false);

        builder.Property(u => u.AccessFailedCount)
            .HasDefaultValue(0);

        builder.Property(u => u.IsApproved)
            .HasDefaultValue(false);

        builder.Property(u => u.IsActive)
            .HasDefaultValue(true);

        builder.Property(u => u.CreatedDate)
            .HasDefaultValueSql("GETUTCDATE()");

        // Navigation properties
        builder.HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Tokens)
            .WithOne(ut => ut.User)
            .HasForeignKey(ut => ut.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.BackupCodes)
            .WithOne(tfb => tfb.User)
            .HasForeignKey(tfb => tfb.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.UserClients)
            .WithOne(uc => uc.User)
            .HasForeignKey(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.AuditLogs)
            .WithOne(al => al.User)
            .HasForeignKey(al => al.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(u => u.Consents)
            .WithOne(uc => uc.User)
            .HasForeignKey(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.DataExportRequests)
            .WithOne(der => der.User)
            .HasForeignKey(der => der.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.AccountDeletionRequests)
            .WithOne(adr => adr.User)
            .HasForeignKey(adr => adr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.AssignedUserClients)
            .WithOne(uc => uc.AssignedByUser)
            .HasForeignKey(uc => uc.AssignedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}