using HelljonVibe.Identity.Models.Entities;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HelljonVibe.Identity.Api.Services;

/// <summary>
/// Interface for JWT token operations.
/// </summary>
public interface ITokenService {
    /// <summary>
    /// Generates a JWT access token for a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="roles">The user's roles.</param>
    /// <returns>The JWT access token.</returns>
    string GenerateAccessToken(User user, string[] roles);

    /// <summary>
    /// Generates a JWT refresh token for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>The JWT refresh token.</returns>
    string GenerateRefreshToken(Guid userId);

    /// <summary>
    /// Validates a JWT access token.
    /// </summary>
    /// <param name="token">The token to validate.</param>
    /// <returns>A tuple containing (isValid, claims).</returns>
    (bool IsValid, ClaimsPrincipal? Principal) ValidateAccessToken(string token);

    /// <summary>
    /// Validates a JWT refresh token.
    /// </summary>
    /// <param name="token">The token to validate.</param>
    /// <returns>A tuple containing (isValid, userId).</returns>
    (bool IsValid, Guid? UserId) ValidateRefreshToken(string token);

    /// <summary>
    /// Gets the user identifier from a JWT token.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <returns>The user identifier.</returns>
    Guid? GetUserIdFromToken(string token);

    /// <summary>
    /// Gets the roles from a JWT token.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <returns>An array of role names.</returns>
    string[] GetRolesFromToken(string token);

    /// <summary>
    /// Revokes a refresh token by adding it to the revocation list.
    /// </summary>
    /// <param name="token">The token to revoke.</param>
    /// <param name="expiry">The optional expiry date for the revocation.</param>
    Task RevokeRefreshTokenAsync(string token, DateTimeOffset? expiry = null);

    /// <summary>
    /// Checks if a refresh token has been revoked.
    /// </summary>
    /// <param name="token">The token to check.</param>
    /// <returns>True if the token has been revoked; otherwise, false.</returns>
    Task<bool> IsRefreshTokenRevokedAsync(string token);

    /// <summary>
    /// Generates a password reset token.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="email">The user's email.</param>
    /// <returns>The password reset token.</returns>
    string GeneratePasswordResetToken(Guid userId, string email);

    /// <summary>
    /// Generates an email confirmation token.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="email">The user's email.</param>
    /// <returns>The email confirmation token.</returns>
    string GenerateEmailConfirmationToken(Guid userId, string email);

    /// <summary>
    /// Validates a password reset token.
    /// </summary>
    /// <param name="token">The token to validate.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="email">The user's email.</param>
    /// <returns>True if the token is valid; otherwise, false.</returns>
    bool ValidatePasswordResetToken(string token, Guid userId, string email);

    /// <summary>
    /// Validates an email confirmation token.
    /// </summary>
    /// <param name="token">The token to validate.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="email">The user's email.</param>
    /// <returns>True if the token is valid; otherwise, false.</returns>
    bool ValidateEmailConfirmationToken(string token, Guid userId, string email);
}
