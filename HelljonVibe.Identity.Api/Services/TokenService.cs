using HelljonVibe.Identity.Api.Configuration;
using HelljonVibe.Identity.Models.Constants;
using HelljonVibe.Identity.Models.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HelljonVibe.Identity.Api.Services;

/// <summary>
/// Implementation of JWT token operations.
/// </summary>
public class TokenService : ITokenService {
    private readonly JwtSettings _jwtSettings;
    private readonly IEncryptionService _encryptionService;
    private readonly HashSet<string> _revokedRefreshTokens = new();
    private static readonly object _lock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenService"/> class.
    /// </summary>
    /// <param name="jwtSettings">The JWT settings.</param>
    /// <param name="encryptionService">The encryption service.</param>
    public TokenService(
        IOptions<JwtSettings> jwtSettings,
        IEncryptionService encryptionService) {
        _jwtSettings = jwtSettings.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
        _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));

        if (string.IsNullOrWhiteSpace(_jwtSettings.Secret)) {
            throw new InvalidOperationException("JWT secret is not configured.");
        }
        if (string.IsNullOrWhiteSpace(_jwtSettings.Issuer)) {
            throw new InvalidOperationException("JWT issuer is not configured.");
        }
        if (string.IsNullOrWhiteSpace(_jwtSettings.Audience)) {
            throw new InvalidOperationException("JWT audience is not configured.");
        }
    }

    /// <summary>
    /// Gets the JWT security key.
    /// </summary>
    private SymmetricSecurityKey SecurityKey => new(Encoding.UTF8.GetBytes(_jwtSettings.Secret!));

    /// <summary>
    /// Gets the JWT signing credentials.
    /// </summary>
    private SigningCredentials SigningCredentials => new(SecurityKey, SecurityAlgorithms.HmacSha256);

    /// <summary>
    /// Gets the JWT token validation parameters.
    /// </summary>
    private TokenValidationParameters TokenValidationParameters => new() {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = _jwtSettings.Issuer,
        ValidAudience = _jwtSettings.Audience,
        IssuerSigningKey = SecurityKey,
        ClockSkew = TimeSpan.Zero
    };

    /// <summary>
    /// Generates a JWT access token for a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="roles">The user's roles.</param>
    /// <returns>The JWT access token.</returns>
    public string GenerateAccessToken(User user, string[] roles) {
        var claims = new List<Claim> {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(Claims.IsApproved, user.IsApproved.ToString().ToLower()),
            new(Claims.IsActive, user.IsActive.ToString().ToLower()),
            new(Claims.EmailConfirmed, user.EmailConfirmed.ToString().ToLower())
        };

        // Add roles
        foreach (var role in roles) {
            claims.Add(new(ClaimTypes.Role, role));
        }

        // Add custom claims
        claims.Add(new(Claims.UserId, user.Id.ToString()));
        claims.Add(new(Claims.Username, user.Username));
        claims.Add(new(Claims.Email, user.Email));

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
            signingCredentials: SigningCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generates a JWT refresh token for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>The JWT refresh token.</returns>
    public string GenerateRefreshToken(Guid userId) {
        var claims = new List<Claim> {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(Claims.TokenType, TokenTypes.Refresh)
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(_jwtSettings.RefreshExpiryInDays),
            signingCredentials: SigningCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Validates a JWT access token.
    /// </summary>
    /// <param name="token">The token to validate.</param>
    /// <returns>A tuple containing (isValid, claims).</returns>
    public (bool IsValid, ClaimsPrincipal? Principal) ValidateAccessToken(string token) {
        try {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, TokenValidationParameters, out _);
            return (true, principal);
        } catch {
            return (false, null);
        }
    }

    /// <summary>
    /// Validates a JWT refresh token.
    /// </summary>
    /// <param name="token">The token to validate.</param>
    /// <returns>A tuple containing (isValid, userId).</returns>
    public (bool IsValid, Guid? UserId) ValidateRefreshToken(string token) {
        try {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, TokenValidationParameters, out _);
            
            var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId)) {
                return (false, null);
            }

            // Check if token has been revoked
            if (IsRefreshTokenRevokedAsync(token).Result) {
                return (false, null);
            }

            return (true, userId);
        } catch {
            return (false, null);
        }
    }

    /// <summary>
    /// Gets the user identifier from a JWT token.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <returns>The user identifier.</returns>
    public Guid? GetUserIdFromToken(string token) {
        try {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub || c.Type == ClaimTypes.NameIdentifier || c.Type == Claims.UserId);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId)) {
                return userId;
            }
            return null;
        } catch {
            return null;
        }
    }

    /// <summary>
    /// Gets the roles from a JWT token.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <returns>An array of role names.</returns>
    public string[] GetRolesFromToken(string token) {
        try {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            
            var roles = jwtToken.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToArray();
            
            return roles;
        } catch {
            return Array.Empty<string>();
        }
    }

    /// <summary>
    /// Revokes a refresh token by adding it to the revocation list.
    /// </summary>
    /// <param name="token">The token to revoke.</param>
    /// <param name="expiry">The optional expiry date for the revocation.</param>
    public Task RevokeRefreshTokenAsync(string token, DateTimeOffset? expiry = null) {
        lock (_lock) {
            _revokedRefreshTokens.Add(token);
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Checks if a refresh token has been revoked.
    /// </summary>
    /// <param name="token">The token to check.</param>
    /// <returns>True if the token has been revoked; otherwise, false.</returns>
    public Task<bool> IsRefreshTokenRevokedAsync(string token) {
        lock (_lock) {
            return Task.FromResult(_revokedRefreshTokens.Contains(token));
        }
    }

    /// <summary>
    /// Generates a password reset token.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="email">The user's email.</param>
    /// <returns>The password reset token.</returns>
    public string GeneratePasswordResetToken(Guid userId, string email) {
        // Create a token with user ID, email, and timestamp
        var tokenData = $"{userId}:{email}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        var encryptedToken = _encryptionService.Encrypt(tokenData);
        
        // Add HMAC signature for tamper detection
        var hmac = ComputeHmac(tokenData);
        
        // Return combined token
        return $"{encryptedToken}:{hmac}";
    }

    /// <summary>
    /// Generates an email confirmation token.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="email">The user's email.</param>
    /// <returns>The email confirmation token.</returns>
    public string GenerateEmailConfirmationToken(Guid userId, string email) {
        // Create a token with user ID, email, and timestamp
        var tokenData = $"{userId}:{email}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        var encryptedToken = _encryptionService.Encrypt(tokenData);
        
        // Add HMAC signature for tamper detection
        var hmac = ComputeHmac(tokenData);
        
        // Return combined token
        return $"{encryptedToken}:{hmac}";
    }

    /// <summary>
    /// Validates a password reset token.
    /// </summary>
    /// <param name="token">The token to validate.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="email">The user's email.</param>
    /// <returns>True if the token is valid; otherwise, false.</returns>
    public bool ValidatePasswordResetToken(string token, Guid userId, string email) {
        try {
            var parts = token.Split(':');
            if (parts.Length < 2) {
                return false;
            }

            var encryptedToken = parts[0];
            var receivedHmac = parts[1];
            
            // Decrypt and verify
            var decryptedToken = _encryptionService.Decrypt(encryptedToken);
            var tokenParts = decryptedToken.Split(':');
            
            if (tokenParts.Length < 3) {
                return false;
            }

            var tokenUserId = tokenParts[0];
            var tokenEmail = tokenParts[1];
            var timestamp = tokenParts[2];
            
            // Verify user ID and email
            if (!Guid.TryParse(tokenUserId, out var parsedUserId) || parsedUserId != userId) {
                return false;
            }
            
            if (tokenEmail != email) {
                return false;
            }

            // Verify HMAC
            var expectedHmac = ComputeHmac(decryptedToken);
            if (receivedHmac != expectedHmac) {
                return false;
            }

            // Check token expiry (24 hours)
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - long.Parse(timestamp) > 86400) {
                return false;
            }

            return true;
        } catch {
            return false;
        }
    }

    /// <summary>
    /// Validates an email confirmation token.
    /// </summary>
    /// <param name="token">The token to validate.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="email">The user's email.</param>
    /// <returns>True if the token is valid; otherwise, false.</returns>
    public bool ValidateEmailConfirmationToken(string token, Guid userId, string email) {
        try {
            var parts = token.Split(':');
            if (parts.Length < 2) {
                return false;
            }

            var encryptedToken = parts[0];
            var receivedHmac = parts[1];
            
            // Decrypt and verify
            var decryptedToken = _encryptionService.Decrypt(encryptedToken);
            var tokenParts = decryptedToken.Split(':');
            
            if (tokenParts.Length < 3) {
                return false;
            }

            var tokenUserId = tokenParts[0];
            var tokenEmail = tokenParts[1];
            var timestamp = tokenParts[2];
            
            // Verify user ID and email
            if (!Guid.TryParse(tokenUserId, out var parsedUserId) || parsedUserId != userId) {
                return false;
            }
            
            if (tokenEmail != email) {
                return false;
            }

            // Verify HMAC
            var expectedHmac = ComputeHmac(decryptedToken);
            if (receivedHmac != expectedHmac) {
                return false;
            }

            // Check token expiry (7 days)
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - long.Parse(timestamp) > 604800) {
                return false;
            }

            return true;
        } catch {
            return false;
        }
    }

    /// <summary>
    /// Computes an HMAC signature for a string.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <returns>The HMAC signature.</returns>
    private string ComputeHmac(string input) {
        using var hmac = new HMACSHA256(SecurityKey.GetBytes());
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hash);
    }
}

/// <summary>
/// Extensions for SecurityKey.
/// </summary>
public static class SecurityKeyExtensions {
    /// <summary>
    /// Gets the bytes of a security key.
    /// </summary>
    /// <param name="key">The security key.</param>
    /// <returns>The key bytes.</returns>
    public static byte[] GetBytes(this SymmetricSecurityKey key) {
        return key.Key;
    }
}
