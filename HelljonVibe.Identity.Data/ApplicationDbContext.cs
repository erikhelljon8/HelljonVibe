using HelljonVibe.Identity.Models.Entities;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace HelljonVibe.Identity.Data;

/// <summary>
/// The main database context for the Identity application.
/// Handles all entity relationships and provides data protection key storage.
/// </summary>
public class ApplicationDbContext : DbContext, IDataProtectionKeyContext {
    /// <summary>
    /// Gets or sets the data protection keys.
    /// Required for ASP.NET Core Data Protection API integration with EF Core.
    /// </summary>
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    /// <summary>
    /// Gets or sets the users.
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Gets or sets the clients.
    /// </summary>
    public DbSet<Client> Clients { get; set; }

    /// <summary>
    /// Gets or sets the roles.
    /// </summary>
    public DbSet<Role> Roles { get; set; }

    /// <summary>
    /// Gets or sets the user roles.
    /// </summary>
    public DbSet<UserRole> UserRoles { get; set; }

    /// <summary>
    /// Gets or sets the user clients.
    /// </summary>
    public DbSet<UserClient> UserClients { get; set; }

    /// <summary>
    /// Gets or sets the user tokens.
    /// </summary>
    public DbSet<UserToken> UserTokens { get; set; }

    /// <summary>
    /// Gets or sets the two-factor backup codes.
    /// </summary>
    public DbSet<TwoFactorBackupCode> TwoFactorBackupCodes { get; set; }

    /// <summary>
    /// Gets or sets the audit logs.
    /// </summary>
    public DbSet<AuditLog> AuditLogs { get; set; }

    /// <summary>
    /// Gets or sets the user consents.
    /// </summary>
    public DbSet<UserConsent> UserConsents { get; set; }

    /// <summary>
    /// Gets or sets the data export requests.
    /// </summary>
    public DbSet<DataExportRequest> DataExportRequests { get; set; }

    /// <summary>
    /// Gets or sets the account deletion requests.
    /// </summary>
    public DbSet<AccountDeletionRequest> AccountDeletionRequests { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
    /// </summary>
    public ApplicationDbContext() {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class with the specified options.
    /// </summary>
    /// <param name="options">The database context options.</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) {
    }

    /// <summary>
    /// Configures the database model.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Configure composite keys
        ConfigureCompositeKeys(modelBuilder);

        // Configure indexes
        ConfigureIndexes(modelBuilder);

        // Configure encryption requirements
        ConfigureEncryptionRequirements(modelBuilder);
    }

    /// <summary>
    /// Configures the composite keys for entities.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    private void ConfigureCompositeKeys(ModelBuilder modelBuilder) {
        // UserRole composite key
        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        // UserClient composite key
        modelBuilder.Entity<UserClient>()
            .HasKey(uc => new { uc.UserId, uc.ClientId });
    }

    /// <summary>
    /// Configures the indexes for entities.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    private void ConfigureIndexes(ModelBuilder modelBuilder) {
        // User indexes
        modelBuilder.Entity<User>()
            .HasIndex(u => u.EncryptedEmail)
            .IsUnique()
            .HasDatabaseName("IX_User_EncryptedEmail");

        modelBuilder.Entity<User>()
            .HasIndex(u => u.EncryptedUsername)
            .IsUnique()
            .HasDatabaseName("IX_User_EncryptedUsername");

        modelBuilder.Entity<User>()
            .HasIndex(u => u.EmailConfirmed)
            .HasDatabaseName("IX_User_EmailConfirmed");

        modelBuilder.Entity<User>()
            .HasIndex(u => u.IsApproved)
            .HasDatabaseName("IX_User_IsApproved");

        modelBuilder.Entity<User>()
            .HasIndex(u => u.IsActive)
            .HasDatabaseName("IX_User_IsActive");

        modelBuilder.Entity<User>()
            .HasIndex(u => u.CreatedDate)
            .HasDatabaseName("IX_User_CreatedDate");

        // Client indexes
        modelBuilder.Entity<Client>()
            .HasIndex(c => c.ClientId)
            .IsUnique()
            .HasDatabaseName("IX_Client_ClientId");

        modelBuilder.Entity<Client>()
            .HasIndex(c => c.IsActive)
            .HasDatabaseName("IX_Client_IsActive");

        // Role indexes
        modelBuilder.Entity<Role>()
            .HasIndex(r => r.Name)
            .IsUnique()
            .HasDatabaseName("IX_Role_Name");

        // UserToken indexes
        modelBuilder.Entity<UserToken>()
            .HasIndex(ut => ut.UserId)
            .HasDatabaseName("IX_UserToken_UserId");

        modelBuilder.Entity<UserToken>()
            .HasIndex(ut => ut.TokenType)
            .HasDatabaseName("IX_UserToken_TokenType");

        modelBuilder.Entity<UserToken>()
            .HasIndex(ut => ut.Expires)
            .HasDatabaseName("IX_UserToken_Expires");

        modelBuilder.Entity<UserToken>()
            .HasIndex(ut => ut.IsUsed)
            .HasDatabaseName("IX_UserToken_IsUsed");

        // TwoFactorBackupCode indexes
        modelBuilder.Entity<TwoFactorBackupCode>()
            .HasIndex(tfb => tfb.UserId)
            .HasDatabaseName("IX_TwoFactorBackupCode_UserId");

        modelBuilder.Entity<TwoFactorBackupCode>()
            .HasIndex(tfb => tfb.IsUsed)
            .HasDatabaseName("IX_TwoFactorBackupCode_IsUsed");

        // AuditLog indexes
        modelBuilder.Entity<AuditLog>()
            .HasIndex(al => al.UserId)
            .HasDatabaseName("IX_AuditLog_UserId");

        modelBuilder.Entity<AuditLog>()
            .HasIndex(al => al.Action)
            .HasDatabaseName("IX_AuditLog_Action");

        modelBuilder.Entity<AuditLog>()
            .HasIndex(al => al.EntityType)
            .HasDatabaseName("IX_AuditLog_EntityType");

        modelBuilder.Entity<AuditLog>()
            .HasIndex(al => al.Timestamp)
            .HasDatabaseName("IX_AuditLog_Timestamp");

        // UserConsent indexes
        modelBuilder.Entity<UserConsent>()
            .HasIndex(uc => uc.UserId)
            .HasDatabaseName("IX_UserConsent_UserId");

        modelBuilder.Entity<UserConsent>()
            .HasIndex(uc => uc.ConsentType)
            .HasDatabaseName("IX_UserConsent_ConsentType");

        modelBuilder.Entity<UserConsent>()
            .HasIndex(uc => uc.ConsentVersion)
            .HasDatabaseName("IX_UserConsent_ConsentVersion");

        // DataExportRequest indexes
        modelBuilder.Entity<DataExportRequest>()
            .HasIndex(der => der.UserId)
            .HasDatabaseName("IX_DataExportRequest_UserId");

        modelBuilder.Entity<DataExportRequest>()
            .HasIndex(der => der.Status)
            .HasDatabaseName("IX_DataExportRequest_Status");

        modelBuilder.Entity<DataExportRequest>()
            .HasIndex(der => der.CreatedDate)
            .HasDatabaseName("IX_DataExportRequest_CreatedDate");

        modelBuilder.Entity<DataExportRequest>()
            .HasIndex(der => der.ExpiresDate)
            .HasDatabaseName("IX_DataExportRequest_ExpiresDate");

        // AccountDeletionRequest indexes
        modelBuilder.Entity<AccountDeletionRequest>()
            .HasIndex(adr => adr.UserId)
            .HasDatabaseName("IX_AccountDeletionRequest_UserId");

        modelBuilder.Entity<AccountDeletionRequest>()
            .HasIndex(adr => adr.Status)
            .HasDatabaseName("IX_AccountDeletionRequest_Status");

        modelBuilder.Entity<AccountDeletionRequest>()
            .HasIndex(adr => adr.CreatedDate)
            .HasDatabaseName("IX_AccountDeletionRequest_CreatedDate");
    }

    /// <summary>
    /// Configures encryption requirements for sensitive fields.
    /// This is a marker for documentation; actual encryption is handled at the application layer.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    private void ConfigureEncryptionRequirements(ModelBuilder modelBuilder) {
        // These fields are encrypted at the application layer before storage
        // User.EncryptedEmail
        // User.EncryptedUsername
        // User.EncryptionKey
        // Client.EncryptedClientSecret
        // UserToken.EncryptedToken
        // TwoFactorBackupCode.EncryptedCode
        // AuditLog.EncryptedDetails
        // UserConsent.EncryptedConsentDocument
        // DataExportRequest.EncryptedFilePath
        // DataExportRequest.EncryptedDownloadToken
        // AccountDeletionRequest.EncryptedConfirmationToken
    }

    /// <summary>
    /// Configures the database connection and other options.
    /// </summary>
    /// <param name="optionsBuilder">The options builder.</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        // Connection string is configured in the Startup/Program class
        // This method is left empty as configuration is done via dependency injection
        base.OnConfiguring(optionsBuilder);
    }
}