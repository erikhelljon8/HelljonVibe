using HelljonVibe.Identity.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace HelljonVibe.Identity.Data.Repositories;

/// <summary>
/// Repository implementation for user-specific operations.
/// </summary>
public class UserRepository : Repository<User>, IUserRepository {
    /// <summary>
    /// Initializes a new instance of the <see cref="UserRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public UserRepository(ApplicationDbContext context)
        : base(context) {
    }

    /// <summary>
    /// Gets a user by their encrypted email address.
    /// </summary>
    /// <param name="encryptedEmail">The encrypted email address.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user or null if not found.</returns>
    public async Task<User?> GetByEmailAsync(string encryptedEmail, CancellationToken cancellationToken = default) {
        return await _dbSet.FirstOrDefaultAsync(u => u.EncryptedEmail == encryptedEmail, cancellationToken);
    }

    /// <summary>
    /// Gets a user by their encrypted username.
    /// </summary>
    /// <param name="encryptedUsername">The encrypted username.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user or null if not found.</returns>
    public async Task<User?> GetByUsernameAsync(string encryptedUsername, CancellationToken cancellationToken = default) {
        return await _dbSet.FirstOrDefaultAsync(u => u.EncryptedUsername == encryptedUsername, cancellationToken);
    }

    /// <summary>
    /// Gets a user by their username or email (encrypted).
    /// </summary>
    /// <param name="encryptedUsernameOrEmail">The encrypted username or email.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user or null if not found.</returns>
    public async Task<User?> GetByUsernameOrEmailAsync(string encryptedUsernameOrEmail, CancellationToken cancellationToken = default) {
        return await _dbSet.FirstOrDefaultAsync(
            u => u.EncryptedUsername == encryptedUsernameOrEmail || u.EncryptedEmail == encryptedUsernameOrEmail,
            cancellationToken);
    }

    /// <summary>
    /// Checks if a user with the specified email exists.
    /// </summary>
    /// <param name="encryptedEmail">The encrypted email address.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if a user with the email exists; otherwise, false.</returns>
    public async Task<bool> EmailExistsAsync(string encryptedEmail, CancellationToken cancellationToken = default) {
        return await _dbSet.AnyAsync(u => u.EncryptedEmail == encryptedEmail, cancellationToken);
    }

    /// <summary>
    /// Checks if a user with the specified username exists.
    /// </summary>
    /// <param name="encryptedUsername">The encrypted username.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if a user with the username exists; otherwise, false.</returns>
    public async Task<bool> UsernameExistsAsync(string encryptedUsername, CancellationToken cancellationToken = default) {
        return await _dbSet.AnyAsync(u => u.EncryptedUsername == encryptedUsername, cancellationToken);
    }

    /// <summary>
    /// Gets users by their approval status.
    /// </summary>
    /// <param name="isApproved">The approval status.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of users with the specified approval status.</returns>
    public async Task<IReadOnlyList<User>> GetByApprovalStatusAsync(bool isApproved, CancellationToken cancellationToken = default) {
        return await _dbSet.Where(u => u.IsApproved == isApproved).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets users by their active status.
    /// </summary>
    /// <param name="isActive">The active status.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of users with the specified active status.</returns>
    public async Task<IReadOnlyList<User>> GetByActiveStatusAsync(bool isActive, CancellationToken cancellationToken = default) {
        return await _dbSet.Where(u => u.IsActive == isActive).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets users by their email verification status.
    /// </summary>
    /// <param name="emailConfirmed">The email verification status.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of users with the specified email verification status.</returns>
    public async Task<IReadOnlyList<User>> GetByEmailConfirmedStatusAsync(bool emailConfirmed, CancellationToken cancellationToken = default) {
        return await _dbSet.Where(u => u.EmailConfirmed == emailConfirmed).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Increments the access failed count for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task IncrementAccessFailedCountAsync(Guid userId, CancellationToken cancellationToken = default) {
        var user = await _dbSet.FindAsync(new object[] { userId }, cancellationToken);
        if (user != null) {
            user.AccessFailedCount++;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Resets the access failed count for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task ResetAccessFailedCountAsync(Guid userId, CancellationToken cancellationToken = default) {
        var user = await _dbSet.FindAsync(new object[] { userId }, cancellationToken);
        if (user != null) {
            user.AccessFailedCount = 0;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Sets the lockout end date for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="lockoutEndDateUtc">The lockout end date.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task SetLockoutEndDateAsync(Guid userId, DateTimeOffset? lockoutEndDateUtc, CancellationToken cancellationToken = default) {
        var user = await _dbSet.FindAsync(new object[] { userId }, cancellationToken);
        if (user != null) {
            user.LockoutEndDateUtc = lockoutEndDateUtc;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Sets the last login date for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task SetLastLoginDateAsync(Guid userId, CancellationToken cancellationToken = default) {
        var user = await _dbSet.FindAsync(new object[] { userId }, cancellationToken);
        if (user != null) {
            user.LastLoginDate = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Sets the email confirmed status for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="emailConfirmed">The email confirmed status.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task SetEmailConfirmedAsync(Guid userId, bool emailConfirmed, CancellationToken cancellationToken = default) {
        var user = await _dbSet.FindAsync(new object[] { userId }, cancellationToken);
        if (user != null) {
            user.EmailConfirmed = emailConfirmed;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Sets the approval status for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="isApproved">The approval status.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task SetApprovalStatusAsync(Guid userId, bool isApproved, CancellationToken cancellationToken = default) {
        var user = await _dbSet.FindAsync(new object[] { userId }, cancellationToken);
        if (user != null) {
            user.IsApproved = isApproved;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Sets the two-factor authentication status for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="twoFactorEnabled">The two-factor authentication status.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task SetTwoFactorEnabledAsync(Guid userId, bool twoFactorEnabled, CancellationToken cancellationToken = default) {
        var user = await _dbSet.FindAsync(new object[] { userId }, cancellationToken);
        if (user != null) {
            user.TwoFactorEnabled = twoFactorEnabled;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Sets the active status for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="isActive">The active status.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task SetActiveStatusAsync(Guid userId, bool isActive, CancellationToken cancellationToken = default) {
        var user = await _dbSet.FindAsync(new object[] { userId }, cancellationToken);
        if (user != null) {
            user.IsActive = isActive;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Gets users with pagination.
    /// </summary>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A tuple containing (users, totalCount).</returns>
    public async Task<(IReadOnlyList<User> Users, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default) {
        var query = _dbSet.OrderBy(u => u.CreatedDate);
        var totalCount = await query.CountAsync(cancellationToken);
        var users = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (users, totalCount);
    }
}
