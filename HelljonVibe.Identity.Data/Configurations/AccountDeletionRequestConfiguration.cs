using HelljonVibe.Identity.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace HelljonVibe.Identity.Data.Configurations;

/// <summary>
/// Entity configuration for the AccountDeletionRequest entity.
/// </summary>
public class AccountDeletionRequestConfiguration : IEntityTypeConfiguration<AccountDeletionRequest> {
    /// <summary>
    /// Configures the AccountDeletionRequest entity.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<AccountDeletionRequest> builder) {
        // Table name
        builder.ToTable("AccountDeletionRequests");

        // Primary key
        builder.HasKey(adr => adr.Id);

        // Required fields
        builder.Property(adr => adr.UserId)
            .IsRequired();

        builder.Property(adr => adr.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Pending");

        builder.Property(adr => adr.DeletionReason)
            .IsRequired()
            .HasMaxLength(100)
            .HasDefaultValue("UserRequest");

        builder.Property(adr => adr.ImmediateDeletion)
            .HasDefaultValue(false);

        builder.Property(adr => adr.EmailConfirmed)
            .HasDefaultValue(false);

        builder.Property(adr => adr.DataAnonymized)
            .HasDefaultValue(false);

        builder.Property(adr => adr.CreatedDate)
            .HasDefaultValueSql("GETUTCDATE()");

        // Optional fields
        builder.Property(adr => adr.UserProvidedReason)
            .HasMaxLength(1000);

        builder.Property(adr => adr.EncryptedConfirmationToken)
            .HasMaxLength(255);

        builder.Property(adr => adr.ConfirmationTokenHash)
            .HasMaxLength(255);

        builder.Property(adr => adr.RequestIpAddress)
            .HasMaxLength(45);

        builder.Property(adr => adr.RequestUserAgent)
            .HasMaxLength(500);

        builder.Property(adr => adr.ErrorMessage)
            .HasMaxLength(1000);

        // Navigation properties
        builder.HasOne(adr => adr.User)
            .WithMany(u => u.AccountDeletionRequests)
            .HasForeignKey(adr => adr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(adr => adr.ProcessedByUser)
            .WithMany()
            .HasForeignKey(adr => adr.ProcessedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
