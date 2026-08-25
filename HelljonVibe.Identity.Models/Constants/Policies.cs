using System;
using System.Collections.Generic;
using System.Text;

namespace HelljonVibe.Identity.Models.Constants;

/// <summary>
/// Defines the authorization policies for the Identity application.
/// </summary>
public static class Policies {
    /// <summary>
    /// Policy for requiring an approved account.
    /// Only users with IsApproved = true can access the application.
    /// </summary>
    public const string RequireApprovedAccount = "RequireApprovedAccount";

    /// <summary>
    /// Policy for requiring email verification.
    /// </summary>
    public const string RequireEmailVerified = "RequireEmailVerified";

    /// <summary>
    /// Policy for requiring administrator role.
    /// </summary>
    public const string RequireAdmin = "RequireAdmin";

    /// <summary>
    /// Policy for requiring user manager role.
    /// </summary>
    public const string RequireUserManager = "RequireUserManager";

    /// <summary>
    /// Policy for requiring client manager role.
    /// </summary>
    public const string RequireClientManager = "RequireClientManager";

    /// <summary>
    /// Policy for requiring auditor role.
    /// </summary>
    public const string RequireAuditor = "RequireAuditor";

    /// <summary>
    /// Policy for requiring any manager role (Admin, UserManager, ClientManager).
    /// </summary>
    public const string RequireManager = "RequireManager";

    /// <summary>
    /// Policy for GDPR data export - users can only export their own data.
    /// </summary>
    public const string GdprDataExport = "GdprDataExport";

    /// <summary>
    /// Policy for GDPR account deletion - users can only delete their own account.
    /// </summary>
    public const string GdprAccountDeletion = "GdprAccountDeletion";

    /// <summary>
    /// Policy for accessing own user data only.
    /// </summary>
    public const string OwnUserDataOnly = "OwnUserDataOnly";

    /// <summary>
    /// Policy for accessing own client data only.
    /// </summary>
    public const string OwnClientDataOnly = "OwnClientDataOnly";
}
