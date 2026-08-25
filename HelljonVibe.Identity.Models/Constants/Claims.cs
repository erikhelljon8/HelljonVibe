using System;
using System.Collections.Generic;
using System.Text;

namespace HelljonVibe.Identity.Models.Constants; 
/// <summary>
/// Defines the custom claim types used in the Identity application.
/// </summary>
public static class Claims {
    /// <summary>
    /// Claim type for user identifier.
    /// </summary>
    public const string UserId = "sub";

    /// <summary>
    /// Claim type for username.
    /// </summary>
    public const string Username = "username";

    /// <summary>
    /// Claim type for email.
    /// </summary>
    public const string Email = "email";

    /// <summary>
    /// Claim type for email verified status.
    /// </summary>
    public const string EmailVerified = "email_verified";

    /// <summary>
    /// Claim type for roles (space-separated list).
    /// </summary>
    public const string Roles = "roles";

    /// <summary>
    /// Claim type for two-factor enabled status.
    /// </summary>
    public const string TwoFactorEnabled = "2fa_enabled";

    /// <summary>
    /// Claim type for account approved status.
    /// </summary>
    public const string IsApproved = "is_approved";

    /// <summary>
    /// Claim type for client ID (when accessing on behalf of a client).
    /// </summary>
    public const string ClientId = "client_id";

    /// <summary>
    /// Claim type for client name.
    /// </summary>
    public const string ClientName = "client_name";

    /// <summary>
    /// Claim type for permissions (space-separated list).
    /// </summary>
    public const string Permissions = "permissions";

    /// <summary>
    /// Claim type for last login date.
    /// </summary>
    public const string LastLoginDate = "last_login_date";

    /// <summary>
    /// Claim type for account creation date.
    /// </summary>
    public const string CreatedDate = "created_date";

    /// <summary>
    /// Claim type for IP address of the current request.
    /// </summary>
    public const string IpAddress = "ip_address";

    /// <summary>
    /// Claim type for user agent of the current request.
    /// </summary>
    public const string UserAgent = "user_agent";

    /// <summary>
    /// Claim type for refresh token identifier.
    /// </summary>
    public const string RefreshTokenId = "refresh_token_id";

    /// <summary>
    /// Claim type for refresh token expiration.
    /// </summary>
    public const string RefreshTokenExpiry = "refresh_token_expiry";
}