using System;
using System.Collections.Generic;
using System.Text;

namespace HelljonVibe.Identity.Models.Constants;

/// <summary>
/// Defines the system roles for the Identity application.
/// </summary>
public static class Roles {
    /// <summary>
    /// Administrator role - has full access to all features.
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// User role - standard user with basic access.
    /// </summary>
    public const string User = "User";

    /// <summary>
    /// Client Manager role - can manage clients and user-client connections.
    /// </summary>
    public const string ClientManager = "ClientManager";

    /// <summary>
    /// User Manager role - can manage users and roles.
    /// </summary>
    public const string UserManager = "UserManager";

    /// <summary>
    /// Audit role - can view audit logs.
    /// </summary>
    public const string Auditor = "Auditor";

    /// <summary>
    /// Gets all system role names.
    /// </summary>
    public static readonly IReadOnlyList<string> All = new List<string>
    {
        Admin,
        User,
        ClientManager,
        UserManager,
        Auditor
    };

    /// <summary>
    /// Gets the system role names that should be created during database seeding.
    /// </summary>
    public static readonly IReadOnlyList<string> SystemRoles = new List<string>
    {
        Admin,
        User
    };
}