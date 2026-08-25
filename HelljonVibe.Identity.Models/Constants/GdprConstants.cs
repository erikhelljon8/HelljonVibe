using System;
using System.Collections.Generic;
using System.Text;

namespace HelljonVibe.Identity.Models.Constants;  
/// <summary>
/// Defines GDPR-related constants for the Identity application.
/// </summary>
public static class GdprConstants {
    /// <summary>
    /// The number of days to retain data export files before automatic deletion.
    /// </summary>
    public const int DataExportRetentionDays = 30;

    /// <summary>
    /// The maximum time (in hours) to process a data export request.
    /// </summary>
    public const int DataExportProcessingHours = 72;

    /// <summary>
    /// The number of days to retain user data after account deletion (for legal compliance).
    /// Set to 0 for immediate deletion, or higher for retention period.
    /// </summary>
    public const int AccountDeletionRetentionDays = 30;

    /// <summary>
    /// The number of days to retain audit logs for GDPR compliance.
    /// </summary>
    public const int AuditLogRetentionDays = 365 * 6; // 6 years

    /// <summary>
    /// The consent version for the current privacy policy.
    /// Increment this when the privacy policy changes.
    /// </summary>
    public const string CurrentConsentVersion = "1.0";

    /// <summary>
    /// The consent type for privacy policy acceptance.
    /// </summary>
    public const string ConsentTypePrivacyPolicy = "PrivacyPolicy";

    /// <summary>
    /// The consent type for marketing communications.
    /// </summary>
    public const string ConsentTypeMarketing = "Marketing";

    /// <summary>
    /// The consent type for analytics tracking.
    /// </summary>
    public const string ConsentTypeAnalytics = "Analytics";

    /// <summary>
    /// The consent type for third-party data sharing.
    /// </summary>
    public const string ConsentTypeThirdPartySharing = "ThirdPartySharing";

    /// <summary>
    /// The data export status when the request is pending.
    /// </summary>
    public const string DataExportStatusPending = "Pending";

    /// <summary>
    /// The data export status when the request is being processed.
    /// </summary>
    public const string DataExportStatusProcessing = "Processing";

    /// <summary>
    /// The data export status when the request is completed and ready for download.
    /// </summary>
    public const string DataExportStatusCompleted = "Completed";

    /// <summary>
    /// The data export status when the request has expired.
    /// </summary>
    public const string DataExportStatusExpired = "Expired";

    /// <summary>
    /// The data export status when the request failed.
    /// </summary>
    public const string DataExportStatusFailed = "Failed";

    /// <summary>
    /// The account deletion status when the request is pending.
    /// </summary>
    public const string AccountDeletionStatusPending = "Pending";

    /// <summary>
    /// The account deletion status when the request is being processed.
    /// </summary>
    public const string AccountDeletionStatusProcessing = "Processing";

    /// <summary>
    /// The account deletion status when the request is completed.
    /// </summary>
    public const string AccountDeletionStatusCompleted = "Completed";

    /// <summary>
    /// The account deletion status when the request failed.
    /// </summary>
    public const string AccountDeletionStatusFailed = "Failed";

    /// <summary>
    /// The reason type for account deletion: user request.
    /// </summary>
    public const string DeletionReasonUserRequest = "UserRequest";

    /// <summary>
    /// The reason type for account deletion: GDPR right to be forgotten.
    /// </summary>
    public const string DeletionReasonGdprRightToBeForgotten = "GdprRightToBeForgotten";

    /// <summary>
    /// The reason type for account deletion: administrative action.
    /// </summary>
    public const string DeletionReasonAdministrative = "Administrative";

    /// <summary>
    /// The reason type for account deletion: terms violation.
    /// </summary>
    public const string DeletionReasonTermsViolation = "TermsViolation";

    /// <summary>
    /// The reason type for account deletion: inactivity.
    /// </summary>
    public const string DeletionReasonInactivity = "Inactivity";

    /// <summary>
    /// The personal data categories that can be exported.
    /// </summary>
    public static class DataCategories {
        /// <summary>
        /// Profile data (username, email, etc.)
        /// </summary>
        public const string Profile = "Profile";

        /// <summary>
        /// Authentication data (login history, tokens, etc.)
        /// </summary>
        public const string Authentication = "Authentication";

        /// <summary>
        /// Client access data (user-client connections)
        /// </summary>
        public const string ClientAccess = "ClientAccess";

        /// <summary>
        /// Role and permission data
        /// </summary>
        public const string RolesAndPermissions = "RolesAndPermissions";

        /// <summary>
        /// Audit log data
        /// </summary>
        public const string AuditLogs = "AuditLogs";

        /// <summary>
        /// Consent data
        /// </summary>
        public const string Consent = "Consent";

        /// <summary>
        /// All personal data
        /// </summary>
        public const string All = "All";
    }
}