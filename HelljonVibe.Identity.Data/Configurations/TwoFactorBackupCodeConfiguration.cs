using HelljonVibe.Identity.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace HelljonVibe.Identity.Data.Configurations;

/// <summary>
/// Entity configuration for the TwoFactorBackupCode entity.
/// </summary>
/// 
public class TwoFactorBackupCodeConfiguration : IEntityTypeConfiguration<TwoFactorBackupCode> {
    /// <summary>
    /// Configures the TwoFactorBackupCode entity.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<TwoFactorBackupCode> builder) {
        // Table name
        builder.ToTable("TwoFactorBackupCodes");

        // Primary key
        builder.HasKey(tfb => tfb.Id);

        // Required fields
        builder.Property(tfb => tfb.UserId)
            .IsRequired();

        builder.Property(tfb => tfb.EncryptedCode)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(tfb => tfb.CodeHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(tfb => tfb.IsUsed)
            .HasDefaultValue(false);

        builder.Property(tfb => tfb.CreatedDate)
            .HasDefaultValueSql("GETUTCDATE()");

        // Navigation properties
        builder.HasOne(tfb => tfb.User)
            .WithMany(u => u.BackupCodes)
            .HasForeignKey(tfb => tfb.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}