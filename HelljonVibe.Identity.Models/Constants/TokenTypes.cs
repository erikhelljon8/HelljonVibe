using System;
using System.Collections.Generic;
using System.Text;

namespace HelljonVibe.Identity.Models.Constants;

/// <summary>
/// Defines the token types used in the Identity application.
/// </summary>
public static class TokenTypes {
    /// <summary>
    /// Token type for email confirmation.
    /// </summary>
    public const string EmailConfirmation = "EmailConfirmation";

    /// <summary>
    /// Token type for password reset.
    /// </summary>
    public const string PasswordReset = "PasswordReset";

    /// <summary>
    /// Token type for refresh tokens.
    /// </summary>
    public const string RefreshToken = "RefreshToken";

    /// <summary>
    /// Token type for 2FA setup.
    /// </summary>
    public const string TwoFactorSetup = "TwoFactorSetup";

    /// <summary>
    /// Token type for account deletion confirmation.
    /// </summary>
    public const string AccountDeletion = "AccountDeletion";

    /// <summary>
    /// Token type for data export.
    /// </summary>
    public const string DataExport = "DataExport";
}