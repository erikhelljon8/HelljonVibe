using HelljonVibe.Identity.Models.Entities;

namespace HelljonVibe.Identity.Api.Services;

/// <summary>
/// Interface for GDPR compliance operations.
/// </summary>
public interface IGdprService {
    /// <summary>
    /// Records user consent.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="consentType">The type of consent.</param>
    /// <param name="isConsentGiven">Whether consent was given.</param>
    /// <param name="version">The version of the consent document.</param>
    /// <param name="content">The content of the consent document.</param>
    /// <param name="ipAddress">The IP address of the request.</param>
    /// <param name="userAgent">The user agent of the request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created consent record.</returns>
    Task<UserConsent> RecordConsentAsync(
        Guid userId,
        string consentType,
        bool isConsentGiven,
        string? version = null,
        string? content = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the consent status for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="consentType">The type of consent.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The consent record or null if not found.</returns>
    Task<UserConsent?> GetConsentAsync(Guid userId, string consentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all consent records for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of consent records.</returns>
    Task<IReadOnlyList<UserConsent>> GetUserConsentsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Withdraws user consent.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="consentType">The type of consent.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the consent was withdrawn successfully; otherwise, false.</returns>
    Task<bool> WithdrawConsentAsync(Guid userId, string consentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests data export for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="requestedDataCategories">The categories of data to export.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created data export request.</returns>
    Task<DataExportRequest> RequestDataExportAsync(
        Guid userId,
        IEnumerable<string>? requestedDataCategories = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a data export request by identifier.
    /// </summary>
    /// <param name="requestId">The request identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The data export request or null if not found.</returns>
    Task<DataExportRequest?> GetDataExportRequestAsync(Guid requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all data export requests for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of data export requests.</returns>
    Task<IReadOnlyList<DataExportRequest>> GetUserDataExportRequestsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports user data.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="dataCategories">The categories of data to export.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A dictionary containing the exported data by category.</returns>
    Task<System.Collections.Generic.Dictionary<string, object>> ExportUserDataAsync(
        Guid userId,
        IEnumerable<string>? dataCategories = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a data export request as completed.
    /// </summary>
    /// <param name="requestId">The request identifier.</param>
    /// <param name="downloadUrl">The download URL for the exported data.</param>
    /// <param name="fileSize">The size of the exported file in bytes.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the request was marked as completed successfully; otherwise, false.</returns>
    Task<bool> MarkDataExportAsCompletedAsync(
        Guid requestId,
        string downloadUrl,
        long fileSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests account deletion.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="reason">The reason for deletion.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created account deletion request.</returns>
    Task<AccountDeletionRequest> RequestAccountDeletionAsync(
        Guid userId,
        string? reason = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an account deletion request by identifier.
    /// </summary>
    /// <param name="requestId">The request identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The account deletion request or null if not found.</returns>
    Task<AccountDeletionRequest?> GetAccountDeletionRequestAsync(Guid requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all account deletion requests for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of account deletion requests.</returns>
    Task<IReadOnlyList<AccountDeletionRequest>> GetUserAccountDeletionRequestsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a user account and all associated data (GDPR right to be forgotten).
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the account was deleted successfully; otherwise, false.</returns>
    Task<bool> DeleteUserAccountAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Anonymizes user data (GDPR right to be forgotten alternative).
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the data was anonymized successfully; otherwise, false.</returns>
    Task<bool> AnonymizeUserDataAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the list of available data categories for export.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of data categories.</returns>
    Task<IReadOnlyList<string>> GetAvailableDataCategoriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has given consent for a specific type.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="consentType">The type of consent.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if consent was given; otherwise, false.</returns>
    Task<bool> HasConsentAsync(Guid userId, string consentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the latest consent version for a specific type.
    /// </summary>
    /// <param name="consentType">The type of consent.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The latest consent version.</returns>
    Task<string?> GetLatestConsentVersionAsync(string consentType, CancellationToken cancellationToken = default);
}
