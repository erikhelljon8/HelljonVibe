using HelljonVibe.Identity.Models.Entities;

namespace HelljonVibe.Identity.Data.Repositories;

/// <summary>
/// Repository interface for user-specific operations.
/// </summary>
public interface IUserRepository : IRepository<User> {
    /// <summary>
    /// Gets a user by their encrypted email address.
    /// </summary>
    /// <param name="encryptedEmail">The encrypted email address.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user or null if not found.</returns>
    Task<User?> GetByEmailAsync(string encryptedEmail, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their encrypted username.
    /// </summary>
    /// <param name="encryptedUsername">The encrypted username.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user or null if not found.</returns>
    Task<User?> GetByUsernameAsync(string encryptedUsername, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their username or email (encrypted).
    /// </summary>
    /// <param name="encryptedUsernameOrEmail">The encrypted username or email.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user or null if not found.</returns>
    Task<User?> GetByUsernameOrEmailAsync(string encryptedUsernameOrEmail, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user with the specified email exists.
    /// </summary>
    /// <param name="encryptedEmail">The encrypted email address.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if a user with the email exists; otherwise, false.</returns>
    Task<bool> EmailExistsAsync(string encryptedEmail, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user with the specified username exists.
    /// </summary>
    /// <param name="encryptedUsername">The encrypted username.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if a user with the username exists; otherwise, false.</returns>
    Task<bool> UsernameExistsAsync(string encryptedUsername, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets users by their approval status.
    /// </summary>
    /// <param name="isApproved">The approval status.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of users with the specified approval status.</returns>
    Task<IReadOnlyList<User>> GetByApprovalStatusAsync(bool isApproved, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets users by their active status.
    /// </summary>
    /// <param name="isActive">The active status.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of users with the specified active status.</returns>
    Task<IReadOnlyList<User>> GetByActiveStatusAsync(bool isActive, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets users by their email verification status.
    /// </summary>
    /// <param name="emailConfirmed">The email verification status.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of users with the specified email verification status.</returns>
    Task<IReadOnlyList<User>> GetByEmailConfirmedStatusAsync(bool emailConfirmed, CancellationToken cancellationToken = default);

    /// <summary>
    /// Increments the access failed count for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task IncrementAccessFailedCountAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets the access failed count for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task ResetAccessFailedCountAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the lockout end date for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="lockoutEndDateUtc">The lockout end date.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task SetLockoutEndDateAsync(Guid userId, DateTimeOffset? lockoutEndDateUtc, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the last login date for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task SetLastLoginDateAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the email confirmed status for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="emailConfirmed">The email confirmed status.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task SetEmailConfirmedAsync(Guid userId, bool emailConfirmed, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the approval status for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="isApproved">The approval status.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task SetApprovalStatusAsync(Guid userId, bool isApproved, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the two-factor authentication status for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="twoFactorEnabled">The two-factor authentication status.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task SetTwoFactorEnabledAsync(Guid userId, bool twoFactorEnabled, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the active status for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="isActive">The active status.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task SetActiveStatusAsync(Guid userId, bool isActive, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets users with pagination.
    /// </summary>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A tuple containing (users, totalCount).</returns>
    Task<(IReadOnlyList<User> Users, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}