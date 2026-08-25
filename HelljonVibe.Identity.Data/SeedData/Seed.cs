using HelljonVibe.Identity.Models.Constants;
using HelljonVibe.Identity.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace HelljonVibe.Identity.Data.SeedData;

/// <summary>
/// Provides seed data for the Identity database.
/// </summary>
public static class Seed {
    /// <summary>
    /// Seeds the database with initial data.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public static async Task InitializeAsync(IServiceProvider serviceProvider) {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await SeedRolesAsync(context);
        await SeedAdminUserAsync(context);
    }

    /// <summary>
    /// Seeds the database with roles.
    /// </summary>
    /// <param name="context">The database context.</param>
    private static async Task SeedRolesAsync(ApplicationDbContext context) {
        if (!await context.Roles.AnyAsync()) {
            var roles = new List<Role>
            {
                new Role
                {
                    Id = Guid.NewGuid(),
                    Name = Roles.Admin,
                    Description = "Administrator role with full access to all features",
                    IsSystemRole = true,
                    CreatedDate = DateTimeOffset.UtcNow
                },
                new Role
                {
                    Id = Guid.NewGuid(),
                    Name = Roles.User,
                    Description = "Standard user role with basic access",
                    IsSystemRole = true,
                    CreatedDate = DateTimeOffset.UtcNow
                },
                new Role
                {
                    Id = Guid.NewGuid(),
                    Name = Roles.ClientManager,
                    Description = "Can manage clients and user-client connections",
                    IsSystemRole = false,
                    CreatedDate = DateTimeOffset.UtcNow
                },
                new Role
                {
                    Id = Guid.NewGuid(),
                    Name = Roles.UserManager,
                    Description = "Can manage users and roles",
                    IsSystemRole = false,
                    CreatedDate = DateTimeOffset.UtcNow
                },
                new Role
                {
                    Id = Guid.NewGuid(),
                    Name = Roles.Auditor,
                    Description = "Can view audit logs",
                    IsSystemRole = false,
                    CreatedDate = DateTimeOffset.UtcNow
                }
            };

            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Seeds the database with an admin user.
    /// </summary>
    /// <param name="context">The database context.</param>
    private static async Task SeedAdminUserAsync(ApplicationDbContext context) {
        // Check if admin user already exists
        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == Roles.Admin);
        if (adminRole == null) {
            return;
        }

        var adminUser = await context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.UserRoles.Any(ur => ur.RoleId == adminRole.Id));

        if (adminUser != null) {
            return;
        }

        // Create admin user
        var encryptedEmail = Encrypt("admin@example.com");
        var encryptedUsername = Encrypt("admin");
        var (passwordHash, passwordSalt) = HashPassword("Admin@123456!");

        adminUser = new User {
            Id = Guid.NewGuid(),
            EncryptedEmail = encryptedEmail,
            EncryptedUsername = encryptedUsername,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            EmailConfirmed = true,
            TwoFactorEnabled = false,
            AccessFailedCount = 0,
            IsApproved = true,
            IsActive = true,
            CreatedDate = DateTimeOffset.UtcNow,
            PrivacyPolicyAcceptedDate = DateTimeOffset.UtcNow,
            PrivacyPolicyVersion = GdprConstants.CurrentConsentVersion,
            TermsAccepted = true,
            TermsAcceptedDate = DateTimeOffset.UtcNow,
            TermsVersion = "1.0"
        };

        await context.Users.AddAsync(adminUser);
        await context.SaveChangesAsync();

        // Assign admin role
        var userRole = new UserRole {
            UserId = adminUser.Id,
            RoleId = adminRole.Id,
            AssignedDate = DateTimeOffset.UtcNow,
            AssignedByUserId = adminUser.Id
        };

        await context.UserRoles.AddAsync(userRole);
        await context.SaveChangesAsync();

        // Create admin consent
        var consent = new UserConsent {
            Id = Guid.NewGuid(),
            UserId = adminUser.Id,
            ConsentType = GdprConstants.ConsentTypePrivacyPolicy,
            ConsentVersion = GdprConstants.CurrentConsentVersion,
            IsConsentGiven = true,
            ConsentGivenDate = DateTimeOffset.UtcNow,
            IsRequired = true,
            CreatedDate = DateTimeOffset.UtcNow
        };

        await context.UserConsents.AddAsync(consent);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Encrypts a string using AES-256-CBC.
    /// Note: This is a placeholder. In production, use a proper encryption service with a secure key.
    /// </summary>
    /// <param name="plainText">The plain text to encrypt.</param>
    /// <returns>The encrypted string.</returns>
    private static string Encrypt(string plainText) {
        // In production, this should use a proper encryption service with a secure key
        // For seeding purposes, we use a simple placeholder
        // DO NOT use this in production without proper key management
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes("0123456789abcdef0123456789abcdef"); // 32 bytes for AES-256
        aes.IV = Encoding.UTF8.GetBytes("0123456789abcdef"); // 16 bytes for AES block size

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new System.IO.MemoryStream();
        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        using var sw = new System.IO.StreamWriter(cs);
        sw.Write(plainText);
        sw.Flush();
        cs.FlushFinalBlock();

        return Convert.ToBase64String(ms.ToArray());
    }

    /// <summary>
    /// Hashes a password using BCrypt.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>A tuple containing (hash, salt).</returns>
    private static (string Hash, string Salt) HashPassword(string password) {
        // In production, use BCrypt.Net-Next
        // For seeding purposes, we use a simple placeholder
        // DO NOT use this in production
        using var sha256 = SHA256.Create();
        var salt = Guid.NewGuid().ToString();
        var saltedPassword = password + salt;
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
        var hash = Convert.ToBase64String(bytes);

        return (hash, salt);
    }
}
