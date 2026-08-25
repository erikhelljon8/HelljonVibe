using HelljonVibe.Identity.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace HelljonVibe.Identity.Data.Configurations;

/// <summary>
/// Entity configuration for the Client entity.
/// </summary>
public class ClientConfiguration : IEntityTypeConfiguration<Client> {
    /// <summary>
    /// Configures the Client entity.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Client> builder) {
        // Table name
        builder.ToTable("Clients");

        // Primary key
        builder.HasKey(c => c.Id);

        // Required fields
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.ClientId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.EncryptedClientSecret)
            .IsRequired()
            .HasMaxLength(512);

        // Default values
        builder.Property(c => c.IsActive)
            .HasDefaultValue(true);

        builder.Property(c => c.CreatedDate)
            .HasDefaultValueSql("GETUTCDATE()");

        // Navigation properties
        builder.HasMany(c => c.UserClients)
            .WithOne(uc => uc.Client)
            .HasForeignKey(uc => uc.ClientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}