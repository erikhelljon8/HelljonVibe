using HelljonVibe.Identity.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace HelljonVibe.Identity.Data.Configurations;

/// <summary>
/// Entity configuration for the DataExportRequest entity.
/// </summary>
public class DataExportRequestConfiguration : IEntityTypeConfiguration<DataExportRequest> {
    /// <summary>
    /// Configures the DataExportRequest entity.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<DataExportRequest> builder) {
        // Table name
        builder.ToTable("DataExportRequests");

        // Primary key
        builder.HasKey(der => der.Id);

        // Required fields
        builder.Property(der => der.UserId)
            .IsRequired();

        builder.Property(der => der.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Pending");

        builder.Property(der => der.ExportFormat)
            .HasMaxLength(20)
            .HasDefaultValue("json");

        builder.Property(der => der.DownloadCount)
            .HasDefaultValue(0);

        builder.Property(der => der.CreatedDate)
            .HasDefaultValueSql("GETUTCDATE()");

        // Optional fields
        builder.Property(der => der.DataCategories)
            .HasMaxLength(500);

        builder.Property(der => der.EncryptedFilePath)
            .HasMaxLength(512);

        builder.Property(der => der.EncryptedDownloadToken)
            .HasMaxLength(255);

        builder.Property(der => der.DownloadTokenHash)
            .HasMaxLength(255);

        builder.Property(der => der.RequestIpAddress)
            .HasMaxLength(45);

        builder.Property(der => der.RequestUserAgent)
            .HasMaxLength(500);

        builder.Property(der => der.ErrorMessage)
            .HasMaxLength(1000);

        // Navigation properties
        builder.HasOne(der => der.User)
            .WithMany(u => u.DataExportRequests)
            .HasForeignKey(der => der.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}