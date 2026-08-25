using HelljonVibe.Identity.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace HelljonVibe.Identity.Data.Configurations;

/// <summary>
/// Entity configuration for the UserToken entity.
/// </summary>
public class UserTokenConfiguration : IEntityTypeConfiguration<UserToken> {
    /// <summary>
    /// Configures the UserToken entity.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<UserToken> builder) {
        // Table name
        builder.ToTable("UserTokens");

        // Primary key
        builder.HasKey(ut => ut.Id);

        // Required fields
        builder.Property(ut => ut.UserId)
            .IsRequired();

        builder.Property(ut => ut.TokenType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ut => ut.EncryptedToken)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(ut => ut.TokenHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(ut => ut.Expires)
            .IsRequired();

        builder.Property(ut => ut.IsUsed)
            .HasDefaultValue(false);

        builder.Property(ut => ut.CreatedDate)
            .HasDefaultValueSql("GETUTCDATE()");

        // Navigation properties
        builder.HasOne(ut => ut.User)
            .WithMany(u => u.Tokens)
            .HasForeignKey(ut => ut.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}