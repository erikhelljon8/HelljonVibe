using HelljonVibe.Identity.Models.DTOs.Requests;
using HelljonVibe.Identity.Models.DTOs.Responses;
using HelljonVibe.Identity.Models.Entities;
using System;
using System.Threading.Tasks;

namespace HelljonVibe.Identity.Api.Services;

/// <summary>
/// Interface for authentication operations.
/// </summary>
public interface IAuthService {
    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="request">The registration request.</param>
    /// <returns>A tuple containing (user, authResponse).</returns>
    Task<(User User, AuthResponse AuthResponse)> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Logs in a user.
    /// </summary>
    /// <param name="request">The login request.</param>
    /// <returns>The authentication response.</returns>
    Task<AuthResponse> LoginAsync(LoginRequest request);

    /// <summary>
    /// Refreshes the access token using a refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token.</param>
    /// <returns>The authentication response with new tokens.</returns>
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Logs out a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="refreshToken">The refresh token to revoke.</param>
    Task LogoutAsync(Guid userId, string refreshToken);

    /// <summary>
    /// Initiates the password reset process.
    /// </summary>
    /// <param name="email">The user's email.</param>
    /// <returns>True if the email was found and reset process initiated.</returns>
    Task<bool> ForgotPasswordAsync(string email);

    /// <summary>
    /// Resets a user's password.
    /// </summary>
    /// <param name="request">The password reset request.</param>
    /// <returns>True if the password was reset successfully.</returns>
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request);

    /// <summary>
    /// Verifies a user's email.
    /// </summary>
    /// <param name="request">The email verification request.</param>
    /// <returns>True if the email was verified successfully.</returns>
    Task<bool> VerifyEmailAsync(VerifyEmailRequest request);

    /// <summary>
    /// Confirms a user's email after they click the verification link.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="token">The email confirmation token.</param>
    /// <returns>True if the email was confirmed successfully.</returns>
    Task<bool> ConfirmEmailAsync(Guid userId, string token);

    /// <summary>
    /// Changes a user's password.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="currentPassword">The current password.</param>
    /// <param name="newPassword">The new password.</param>
    /// <returns>True if the password was changed successfully.</returns>
    Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);

    /// <summary>
    /// Checks if a user exists with the specified email.
    /// </summary>
    /// <param name="email">The email to check.</param>
    /// <returns>True if a user exists with the email.</returns>
    Task<bool> UserExistsAsync(string email);

    /// <summary>
    /// Checks if a user exists with the specified username.
    /// </summary>
    /// <param name="username">The username to check.</param>
    /// <returns>True if a user exists with the username.</returns>
    Task<bool> UsernameExistsAsync(string username);

    /// <summary>
    /// Gets the current user's information.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>The user response.</returns>
    Task<UserResponse> GetCurrentUserAsync(Guid userId);
}
