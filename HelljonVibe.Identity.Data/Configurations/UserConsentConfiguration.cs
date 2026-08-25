using HelljonVibe.Identity.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace HelljonVibe.Identity.Data.Configurations;

/// <summary>
/// Entity configuration for the UserConsent entity.
/// </summary>
public class UserConsentConfiguration : IEntityTypeConfiguration<UserConsent> {
    /// <summary>
    /// Configures the UserConsent entity.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<UserConsent> builder) {
        // Table name
        builder.ToTable("UserConsents");

        // Primary key
        builder.HasKey(uc => uc.Id);

        // Required fields
        builder.Property(uc => uc.UserId)
            .IsRequired();

        builder.Property(uc => uc.ConsentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(uc => uc.ConsentVersion)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(uc => uc.IsConsentGiven)
            .HasDefaultValue(false);

        builder.Property(uc => uc.IsRequired)
            .HasDefaultValue(false);

        builder.Property(uc => uc.CreatedDate)
            .HasDefaultValueSql("GETUTCDATE()");

        // Optional fields
        builder.Property(uc => uc.ConsentIpAddress)
            .HasMaxLength(45);

        builder.Property(uc => uc.ConsentUserAgent)
            .HasMaxLength(500);

        builder.Property(uc => uc.EncryptedConsentDocument)
            .HasMaxLength(4000);

        // Navigation properties
        builder.HasOne(uc => uc.User)
            .WithMany(u => u.Consents)
            .HasForeignKey(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}