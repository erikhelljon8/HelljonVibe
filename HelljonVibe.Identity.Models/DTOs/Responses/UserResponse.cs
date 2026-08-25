using System;
using System.Collections.Generic;
using System.Text;

namespace HelljonVibe.Identity.Models.DTOs.Responses;

/// <summary>
/// DTO for user responses.
/// </summary>
public class UserResponse {
    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the email is verified.
    /// </summary>
    public bool EmailVerified { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether two-factor authentication is enabled.
    /// </summary>
    public bool TwoFactorEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the account is approved.
    /// </summary>
    public bool IsApproved { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the account is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user has accepted the terms of service.
    /// </summary>
    public bool TermsAccepted { get; set; }

    /// <summary>
    /// Gets or sets the version of the terms of service that the user accepted.
    /// </summary>
    public string? TermsVersion { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the user accepted the terms of service.
    /// </summary>
    public DateTimeOffset? TermsAcceptedDate { get; set; }

    /// <summary>
    /// Gets or sets the version of the privacy policy that the user accepted.
    /// </summary>
    public string? PrivacyPolicyVersion { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the user accepted the privacy policy.
    /// </summary>
    public DateTimeOffset? PrivacyPolicyAcceptedDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the user was created.
    /// </summary>
    public DateTimeOffset CreatedDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time of the last login.
    /// </summary>
    public DateTimeOffset? LastLoginDate { get; set; }

    /// <summary>
    /// Gets or sets the number of failed login attempts.
    /// </summary>
    public int AccessFailedCount { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the user was locked out.
    /// </summary>
    public DateTimeOffset? LockoutEndDateUtc { get; set; }

    /// <summary>
    /// Gets or sets the roles assigned to the user.
    /// </summary>
    public IList<string> Roles { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the clients that the user has access to.
    /// </summary>
    public IList<ClientResponse> Clients { get; set; } = new List<ClientResponse>();

    /// <summary>
    /// Gets or sets the user's consent information.
    /// </summary>
    public IList<UserConsentResponse> Consents { get; set; } = new List<UserConsentResponse>();
}

/// <summary>
/// DTO for client responses in user context.
/// </summary>
public class ClientResponse {
    /// <summary>
    /// Gets or sets the client identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the client.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the client identifier (ClientId).
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the client is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the user was assigned to the client.
    /// </summary>
    public DateTimeOffset AssignedDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user's access to this client is active.
    /// </summary>
    public bool IsAccessActive { get; set; }
}

/// <summary>
/// DTO for user consent responses.
/// </summary>
public class UserConsentResponse {
    /// <summary>
    /// Gets or sets the consent type.
    /// </summary>
    public string ConsentType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the consent version.
    /// </summary>
    public string ConsentVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether consent was given.
    /// </summary>
    public bool IsConsentGiven { get; set; }

    /// <summary>
    /// Gets or sets the date and time when consent was given.
    /// </summary>
    public DateTimeOffset? ConsentGivenDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when consent was withdrawn.
    /// </summary>
    public DateTimeOffset? ConsentWithdrawnDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this consent is required.
    /// </summary>
    public bool IsRequired { get; set; }
}
