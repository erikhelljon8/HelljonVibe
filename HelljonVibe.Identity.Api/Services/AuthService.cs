using HelljonVibe.Identity.Api.Configuration;
using HelljonVibe.Identity.Data;
using HelljonVibe.Identity.Data.Repositories;
using HelljonVibe.Identity.Models.Constants;
using HelljonVibe.Identity.Models.DTOs.Requests;
using HelljonVibe.Identity.Models.DTOs.Responses;
using HelljonVibe.Identity.Models.Entities;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HelljonVibe.Identity.Api.Services;

/// <summary>
/// Implementation of authentication operations.
/// </summary>
public class AuthService : IAuthService {
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEncryptionService _encryptionService;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly AppSettings _appSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthService"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="encryptionService">The encryption service.</param>
    /// <param name="tokenService">The token service.</param>
    /// <param name="emailService">The email service.</param>
    /// <param name="appSettings">The application settings.</param>
    public AuthService(
        IUnitOfWork unitOfWork,
        IEncryptionService encryptionService,
        ITokenService tokenService,
        IEmailService emailService,
        IOptions<AppSettings> appSettings) {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _appSettings = appSettings.Value ?? throw new ArgumentNullException(nameof(appSettings));
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="request">The registration request.</param>
    /// <returns>A tuple containing (user, authResponse).</returns>
    public async Task<(User User, AuthResponse AuthResponse)> RegisterAsync(RegisterRequest request) {
        // Check if username or email already exists
        var encryptedUsername = _encryptionService.Encrypt(request.Username);
        var encryptedEmail = _encryptionService.Encrypt(request.Email);

        var usernameExists = await _unitOfWork.Users.UsernameExistsAsync(encryptedUsername);
        var emailExists = await _unitOfWork.Users.EmailExistsAsync(encryptedEmail);

        if (usernameExists) {
            throw new InvalidOperationException("Username already exists.");
        }

        if (emailExists) {
            throw new InvalidOperationException("Email already exists.");
        }

        // Hash password
        var (passwordHash, passwordSalt) = _encryptionService.HashPassword(request.Password);

        // Generate user encryption key
        var userKey = _encryptionService.GenerateUserKey();

        // Create user
        var user = new User {
            Id = Guid.NewGuid(),
            EncryptedEmail = encryptedEmail,
            EncryptedUsername = encryptedUsername,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            EmailConfirmed = false,
            TwoFactorEnabled = false,
            AccessFailedCount = 0,
            IsApproved = !_appSettings.RequireClientSideEncryption, // Auto-approve if not requiring client-side encryption
            IsActive = true,
            CreatedDate = DateTimeOffset.UtcNow,
            EncryptionKey = userKey,
            PrivacyPolicyAcceptedDate = DateTimeOffset.UtcNow,
            PrivacyPolicyVersion = GdprConstants.CurrentConsentVersion,
            TermsAccepted = true,
            TermsAcceptedDate = DateTimeOffset.UtcNow,
            TermsVersion = "1.0"
        };

        // Add user to database
        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.CommitAsync();

        // Assign default User role
        var userRole = await _unitOfWork.Roles.GetByNameAsync(Roles.User);
        if (userRole != null) {
            var roleAssignment = new UserRole {
                UserId = user.Id,
                RoleId = userRole.Id,
                AssignedDate = DateTimeOffset.UtcNow,
                AssignedByUserId = user.Id
            };
            await _unitOfWork.UserRoles.AddAsync(roleAssignment);
            await _unitOfWork.CommitAsync();
        }

        // Generate email confirmation token
        var emailToken = _tokenService.GenerateEmailConfirmationToken(user.Id, request.Email);

        // Send welcome email with confirmation link
        var confirmationLink = $"{_appSettings.FrontendUrl}/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(emailToken)}";
        await _emailService.SendWelcomeEmailAsync(request.Email, request.Username, confirmationLink);

        // Generate auth response with tokens
        var roles = new[] { Roles.User };
        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var refreshToken = _tokenService.GenerateRefreshToken(user.Id);

        var authResponse = new AuthResponse {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = _appSettings.ExpiryInMinutes * 60,
            TokenType = "Bearer",
            UserId = user.Id,
            Username = request.Username,
            Email = request.Email,
            Roles = roles,
            IsApproved = user.IsApproved,
            RequiresEmailConfirmation = !user.EmailConfirmed,
            RequiresApproval = !user.IsApproved
        };

        return (user, authResponse);
    }

    /// <summary>
    /// Logs in a user.
    /// </summary>
    /// <param name="request">The login request.</param>
    /// <returns>The authentication response.</returns>
    public async Task<AuthResponse> LoginAsync(LoginRequest request) {
        // Find user by username or email
        var encryptedUsernameOrEmail = _encryptionService.Encrypt(request.UsernameOrEmail);
        var user = await _unitOfWork.Users.GetByUsernameOrEmailAsync(encryptedUsernameOrEmail);

        if (user == null) {
            // Log failed attempt (but don't reveal user existence)
            await _unitOfWork.Users.IncrementAccessFailedCountAsync(Guid.Empty);
            throw new UnauthorizedAccessException("Invalid username/email or password.");
        }

        // Check if account is locked out
        if (user.LockoutEndDateUtc.HasValue && user.LockoutEndDateUtc > DateTimeOffset.UtcNow) {
            throw new UnauthorizedAccessException("Account is temporarily locked due to too many failed login attempts.");
        }

        // Check if account is active
        if (!user.IsActive) {
            throw new UnauthorizedAccessException("Account is inactive.");
        }

        // Check if account is approved
        if (!user.IsApproved) {
            throw new UnauthorizedAccessException("Account is not approved yet.");
        }

        // Verify password
        var isPasswordValid = _encryptionService.VerifyPassword(
            request.Password,
            user.PasswordHash,
            user.PasswordSalt);

        if (!isPasswordValid) {
            // Increment failed access count
            await _unitOfWork.Users.IncrementAccessFailedCountAsync(user.Id);

            // Check if account should be locked
            if (user.AccessFailedCount >= _appSettings.MaxFailedAccessAttempts) {
                await _unitOfWork.Users.SetLockoutEndDateAsync(
                    user.Id,
                    DateTimeOffset.UtcNow.AddMinutes(_appSettings.LockoutDurationInMinutes));
                await _unitOfWork.CommitAsync();
            }

            throw new UnauthorizedAccessException("Invalid username/email or password.");
        }

        // Reset failed access count on successful login
        await _unitOfWork.Users.ResetAccessFailedCountAsync(user.Id);
        await _unitOfWork.Users.SetLockoutEndDateAsync(user.Id, null);
        await _unitOfWork.Users.SetLastLoginDateAsync(user.Id);
        await _unitOfWork.CommitAsync();

        // Get user roles
        var userRoles = await _unitOfWork.UserRoles.GetByUserIdAsync(user.Id);
        var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
        var roles = await _unitOfWork.Roles.GetByIdsAsync(roleIds);
        var roleNames = roles.Select(r => r.Name).ToArray();

        // Decrypt user data for response
        var username = _encryptionService.Decrypt(user.EncryptedUsername);
        var email = _encryptionService.Decrypt(user.EncryptedEmail);

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user, roleNames);
        var refreshToken = _tokenService.GenerateRefreshToken(user.Id);

        return new AuthResponse {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = _appSettings.ExpiryInMinutes * 60,
            TokenType = "Bearer",
            UserId = user.Id,
            Username = username,
            Email = email,
            Roles = roleNames,
            IsApproved = user.IsApproved,
            RequiresEmailConfirmation = !user.EmailConfirmed,
            RequiresApproval = !user.IsApproved
        };
    }

    /// <summary>
    /// Refreshes the access token using a refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token.</param>
    /// <returns>The authentication response with new tokens.</returns>
    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken) {
        // Validate refresh token
        var (isValid, userId) = _tokenService.ValidateRefreshToken(refreshToken);
        
        if (!isValid || !userId.HasValue) {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        // Get user
        var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);
        if (user == null || !user.IsActive || !user.IsApproved) {
            throw new UnauthorizedAccessException("Invalid user account.");
        }

        // Get user roles
        var userRoles = await _unitOfWork.UserRoles.GetByUserIdAsync(user.Id);
        var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
        var roles = await _unitOfWork.Roles.GetByIdsAsync(roleIds);
        var roleNames = roles.Select(r => r.Name).ToArray();

        // Decrypt user data
        var username = _encryptionService.Decrypt(user.EncryptedUsername);
        var email = _encryptionService.Decrypt(user.EncryptedEmail);

        // Generate new tokens
        var newAccessToken = _tokenService.GenerateAccessToken(user, roleNames);
        var newRefreshToken = _tokenService.GenerateRefreshToken(user.Id);

        // Revoke old refresh token
        await _tokenService.RevokeRefreshTokenAsync(refreshToken);

        return new AuthResponse {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = _appSettings.ExpiryInMinutes * 60,
            TokenType = "Bearer",
            UserId = user.Id,
            Username = username,
            Email = email,
            Roles = roleNames,
            IsApproved = user.IsApproved,
            RequiresEmailConfirmation = !user.EmailConfirmed,
            RequiresApproval = !user.IsApproved
        };
    }

    /// <summary>
    /// Logs out a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="refreshToken">The refresh token to revoke.</param>
    public async Task LogoutAsync(Guid userId, string refreshToken) {
        // Revoke refresh token
        await _tokenService.RevokeRefreshTokenAsync(refreshToken);

        // Additional logout logic can be added here (e.g., clearing session data)
    }

    /// <summary>
    /// Initiates the password reset process.
    /// </summary>
    /// <param name="email">The user's email.</param>
    /// <returns>True if the email was found and reset process initiated.</returns>
    public async Task<bool> ForgotPasswordAsync(string email) {
        var encryptedEmail = _encryptionService.Encrypt(email);
        var user = await _unitOfWork.Users.GetByEmailAsync(encryptedEmail);

        if (user == null) {
            // Don't reveal user existence
            return true;
        }

        // Generate password reset token
        var decryptedEmail = _encryptionService.Decrypt(user.EncryptedEmail);
        var token = _tokenService.GeneratePasswordResetToken(user.Id, decryptedEmail);

        // Send password reset email
        var resetLink = $"{_appSettings.FrontendUrl}/reset-password?userId={user.Id}&token={Uri.EscapeDataString(token)}";
        await _emailService.SendPasswordResetEmailAsync(decryptedEmail, user.Username, resetLink);

        return true;
    }

    /// <summary>
    /// Resets a user's password.
    /// </summary>
    /// <param name="request">The password reset request.</param>
    /// <returns>True if the password was reset successfully.</returns>
    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request) {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
        if (user == null) {
            return false;
        }

        var decryptedEmail = _encryptionService.Decrypt(user.EncryptedEmail);
        
        // Validate token
        var isTokenValid = _tokenService.ValidatePasswordResetToken(
            request.Token,
            user.Id,
            decryptedEmail);

        if (!isTokenValid) {
            return false;
        }

        // Hash new password
        var (passwordHash, passwordSalt) = _encryptionService.HashPassword(request.NewPassword);

        // Update user
        user.PasswordHash = passwordHash;
        user.PasswordSalt = passwordSalt;
        user.AccessFailedCount = 0;
        user.LockoutEndDateUtc = null;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.CommitAsync();

        // Send password reset confirmation email
        await _emailService.SendPasswordResetConfirmationEmailAsync(
            decryptedEmail,
            user.Username);

        return true;
    }

    /// <summary>
    /// Verifies a user's email.
    /// </summary>
    /// <param name="request">The email verification request.</param>
    /// <returns>True if the email was verified successfully.</returns>
    public async Task<bool> VerifyEmailAsync(VerifyEmailRequest request) {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
        if (user == null) {
            return false;
        }

        var decryptedEmail = _encryptionService.Decrypt(user.EncryptedEmail);
        
        // Validate token
        var isTokenValid = _tokenService.ValidateEmailConfirmationToken(
            request.Token,
            user.Id,
            decryptedEmail);

        if (!isTokenValid) {
            return false;
        }

        return await ConfirmEmailAsync(user.Id, request.Token);
    }

    /// <summary>
    /// Confirms a user's email after they click the verification link.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="token">The email confirmation token.</param>
    /// <returns>True if the email was confirmed successfully.</returns>
    public async Task<bool> ConfirmEmailAsync(Guid userId, string token) {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) {
            return false;
        }

        if (user.EmailConfirmed) {
            return true; // Already confirmed
        }

        var decryptedEmail = _encryptionService.Decrypt(user.EncryptedEmail);
        
        // Validate token
        var isTokenValid = _tokenService.ValidateEmailConfirmationToken(
            token,
            user.Id,
            decryptedEmail);

        if (!isTokenValid) {
            return false;
        }

        // Mark email as confirmed
        await _unitOfWork.Users.SetEmailConfirmedAsync(user.Id, true);
        await _unitOfWork.CommitAsync();

        // Create consent record
        var consent = new UserConsent {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            ConsentType = GdprConstants.ConsentTypePrivacyPolicy,
            ConsentVersion = GdprConstants.CurrentConsentVersion,
            IsConsentGiven = true,
            ConsentGivenDate = DateTimeOffset.UtcNow,
            IsRequired = true,
            CreatedDate = DateTimeOffset.UtcNow
        };
        await _unitOfWork.UserConsents.AddAsync(consent);
        await _unitOfWork.CommitAsync();

        return true;
    }

    /// <summary>
    /// Changes a user's password.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="currentPassword">The current password.</param>
    /// <param name="newPassword">The new password.</param>
    /// <returns>True if the password was changed successfully.</returns>
    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword) {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) {
            return false;
        }

        // Verify current password
        var isCurrentPasswordValid = _encryptionService.VerifyPassword(
            currentPassword,
            user.PasswordHash,
            user.PasswordSalt);

        if (!isCurrentPasswordValid) {
            return false;
        }

        // Hash new password
        var (passwordHash, passwordSalt) = _encryptionService.HashPassword(newPassword);

        // Update user
        user.PasswordHash = passwordHash;
        user.PasswordSalt = passwordSalt;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.CommitAsync();

        return true;
    }

    /// <summary>
    /// Checks if a user exists with the specified email.
    /// </summary>
    /// <param name="email">The email to check.</param>
    /// <returns>True if a user exists with the email.</returns>
    public async Task<bool> UserExistsAsync(string email) {
        var encryptedEmail = _encryptionService.Encrypt(email);
        return await _unitOfWork.Users.EmailExistsAsync(encryptedEmail);
    }

    /// <summary>
    /// Checks if a user exists with the specified username.
    /// </summary>
    /// <param name="username">The username to check.</param>
    /// <returns>True if a user exists with the username.</returns>
    public async Task<bool> UsernameExistsAsync(string username) {
        var encryptedUsername = _encryptionService.Encrypt(username);
        return await _unitOfWork.Users.UsernameExistsAsync(encryptedUsername);
    }

    /// <summary>
    /// Gets the current user's information.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>The user response.</returns>
    public async Task<UserResponse> GetCurrentUserAsync(Guid userId) {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) {
            throw new KeyNotFoundException("User not found.");
        }

        // Get user roles
        var userRoles = await _unitOfWork.UserRoles.GetByUserIdAsync(user.Id);
        var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
        var roles = await _unitOfWork.Roles.GetByIdsAsync(roleIds);
        var roleNames = roles.Select(r => r.Name).ToArray();

        // Decrypt user data
        var username = _encryptionService.Decrypt(user.EncryptedUsername);
        var email = _encryptionService.Decrypt(user.EncryptedEmail);

        return new UserResponse {
            Id = user.Id,
            Username = username,
            Email = email,
            EmailConfirmed = user.EmailConfirmed,
            TwoFactorEnabled = user.TwoFactorEnabled,
            IsApproved = user.IsApproved,
            IsActive = user.IsActive,
            CreatedDate = user.CreatedDate,
            LastLoginDate = user.LastLoginDate,
            Roles = roleNames,
            PrivacyPolicyAccepted = user.PrivacyPolicyAcceptedDate.HasValue,
            TermsAccepted = user.TermsAccepted
        };
    }
}

/// <summary>
/// Extensions for IRepository to support role operations.
/// </summary>
public static class RepositoryExtensions {
    /// <summary>
    /// Gets a role by name.
    /// </summary>
    /// <param name="repository">The repository.</param>
    /// <param name="name">The role name.</param>
    /// <returns>The role or null if not found.</returns>
    public static async Task<Role?> GetByNameAsync(this IRepository<Role> repository, string name) {
        var dbSet = repository.GetDbSet();
        return await dbSet.FirstOrDefaultAsync(r => r.Name == name);
    }

    /// <summary>
    /// Gets roles by IDs.
    /// </summary>
    /// <param name="repository">The repository.</param>
    /// <param name="ids">The role IDs.</param>
    /// <returns>A list of roles.</returns>
    public static async Task<List<Role>> GetByIdsAsync(this IRepository<Role> repository, List<Guid> ids) {
        var dbSet = repository.GetDbSet();
        return await dbSet.Where(r => ids.Contains(r.Id)).ToListAsync();
    }

    /// <summary>
    /// Gets user roles by user ID.
    /// </summary>
    /// <param name="repository">The repository.</param>
    /// <param name="userId">The user ID.</param>
    /// <returns>A list of user roles.</returns>
    public static async Task<List<UserRole>> GetByUserIdAsync(this IRepository<UserRole> repository, Guid userId) {
        var dbSet = repository.GetDbSet();
        return await dbSet.Where(ur => ur.UserId == userId).ToListAsync();
    }
}
