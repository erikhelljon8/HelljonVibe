using HelljonVibe.Identity.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace HelljonVibe.Identity.Data.Configurations;

/// <summary>
/// Entity configuration for the AuditLog entity.
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog> {
    /// <summary>
    /// Configures the AuditLog entity.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<AuditLog> builder) {
        // Table name
        builder.ToTable("AuditLogs");

        // Primary key
        builder.HasKey(al => al.Id);

        // Required fields
        builder.Property(al => al.Action)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(al => al.EntityType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(al => al.Timestamp)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(al => al.IsSuccess)
            .HasDefaultValue(true);

        // Optional fields
        builder.Property(al => al.EntityId)
            .HasMaxLength(100);

        builder.Property(al => al.IpAddress)
            .HasMaxLength(45);

        builder.Property(al => al.UserAgent)
            .HasMaxLength(500);

        builder.Property(al => al.EncryptedDetails)
            .HasMaxLength(4000);

        builder.Property(al => al.ErrorMessage)
            .HasMaxLength(1000);

        // Navigation properties
        builder.HasOne(al => al.User)
            .WithMany(u => u.AuditLogs)
            .HasForeignKey(al => al.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
