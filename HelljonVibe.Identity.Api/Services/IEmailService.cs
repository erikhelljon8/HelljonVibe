using HelljonVibe.Identity.Models.Entities;

namespace HelljonVibe.Identity.Api.Services;

/// <summary>
/// Interface for email operations.
/// </summary>
public interface IEmailService {

    /// <summary>
    /// Sends a welcome email to a newly registered user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="verificationToken">The email verification token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task SendWelcomeEmailAsync(User user, string verificationToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a password reset email.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="resetToken">The password reset token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task SendPasswordResetEmailAsync(User user, string resetToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email verification email.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="verificationToken">The verification token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task SendEmailVerificationEmailAsync(User user, string verificationToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a 2FA setup email.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="setupUrl">The 2FA setup URL.</param>
    /// <param name="backupCodes">The backup codes.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task SendTwoFactorSetupEmailAsync(User user, string setupUrl, System.Collections.Generic.IList<string> backupCodes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an account deletion confirmation email.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="confirmationToken">The confirmation token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task SendAccountDeletionEmailAsync(User user, string confirmationToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a data export ready email.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="downloadUrl">The download URL for the exported data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task SendDataExportReadyEmailAsync(User user, string downloadUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a generic email notification.
    /// </summary>
    /// <param name="email">The recipient email address.</param>
    /// <param name="subject">The email subject.</param>
    /// <param name="message">The email message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task SendEmailAsync(string email, string subject, string message, CancellationToken cancellationToken = default);
}
