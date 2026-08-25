using HelljonVibe.Identity.Api.Configuration;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HelljonVibe.Identity.Api.Services;

/// <summary>
/// Implementation of email operations using MailKit.
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailService"/> class.
    /// </summary>
    /// <param name="settings">The email settings.</param>
    /// <param name="logger">The logger.</param>
    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService>? logger = null)
    {
        _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger;
    }

    /// <summary>
    /// Sends a welcome email to a new user.
    /// </summary>
    /// <param name="email">The recipient email address.</param>
    /// <param name="username">The recipient username.</param>
    /// <param name="confirmationLink">The email confirmation link.</param>
    public async Task SendWelcomeEmailAsync(string email, string username, string confirmationLink)
    {
        var subject = "Welcome to HelljonVibe Identity";
        var body = $@"<html><body><h1>Welcome, {username}!</h1><p>Thank you for registering with HelljonVibe Identity.</p><p>Please confirm your email address by clicking the link below:</p><p><a href="{confirmationLink}">Confirm Email</a></p><p>If you didn't request this, please ignore this email.</p><p>Best regards,<br>The HelljonVibe Team</p></body></html>";
        await SendEmailAsync(email, subject, body);
    }

    /// <summary>
    /// Sends a password reset email.
    /// </summary>
    /// <param name="email">The recipient email address.</param>
    /// <param name="username">The recipient username.</param>
    /// <param name="resetLink">The password reset link.</param>
    public async Task SendPasswordResetEmailAsync(string email, string username, string resetLink)
    {
        var subject = "Password Reset Request";
        var body = $@"<html><body><h1>Password Reset</h1><p>Hello {username},</p><p>We received a request to reset your password. Click the link below to reset it:</p><p><a href="{resetLink}">Reset Password</a></p><p>If you didn't request this, please ignore this email or contact support.</p><p>This link will expire in 24 hours.</p><p>Best regards,<br>The HelljonVibe Team</p></body></html>";
        await SendEmailAsync(email, subject, body);
    }

    /// <summary>
    /// Sends a password reset confirmation email.
    /// </summary>
    /// <param name="email">The recipient email address.</param>
    /// <param name="username">The recipient username.</param>
    public async Task SendPasswordResetConfirmationEmailAsync(string email, string username)
    {
        var subject = "Password Changed";
        var body = $@"<html><body><h1>Password Changed</h1><p>Hello {username},</p><p>Your password has been successfully changed.</p><p>If you didn't request this change, please contact support immediately.</p><p>Best regards,<br>The HelljonVibe Team</p></body></html>";
        await SendEmailAsync(email, subject, body);
    }

    /// <summary>
    /// Sends an email confirmation email.
    /// </summary>
    /// <param name="email">The recipient email address.</param>
    /// <param name="username">The recipient username.</param>
    /// <param name="confirmationLink">The email confirmation link.</param>
    public async Task SendEmailConfirmationEmailAsync(string email, string username, string confirmationLink)
    {
        var subject = "Confirm Your Email Address";
        var body = $@"<html><body><h1>Confirm Your Email</h1><p>Hello {username},</p><p>Please confirm your email address by clicking the link below:</p><p><a href="{confirmationLink}">Confirm Email</a></p><p>If you didn't request this, please ignore this email.</p><p>Best regards,<br>The HelljonVibe Team</p></body></html>";
        await SendEmailAsync(email, subject, body);
    }

    /// <summary>
    /// Sends a two-factor authentication setup email.
    /// </summary>
    /// <param name="email">The recipient email address.</param>
    /// <param name="username">The recipient username.</param>
    /// <param name="setupLink">The 2FA setup link.</param>
    public async Task SendTwoFactorSetupEmailAsync(string email, string username, string setupLink)
    {
        var subject = "Set Up Two-Factor Authentication";
        var body = $@"<html><body><h1>Set Up Two-Factor Authentication</h1><p>Hello {username},</p><p>To enhance your account security, please set up two-factor authentication:</p><p><a href="{setupLink}">Set Up 2FA</a></p><p>Best regards,<br>The HelljonVibe Team</p></body></html>";
        await SendEmailAsync(email, subject, body);
    }

    /// <summary>
    /// Sends a two-factor authentication code.
    /// </summary>
    /// <param name="email">The recipient email address.</param>
    /// <param name="code">The 2FA code.</param>
    public async Task SendTwoFactorCodeEmailAsync(string email, string code)
    {
        var subject = "Your Two-Factor Authentication Code";
        var body = $@"<html><body><h1>Your 2FA Code</h1><p>Your two-factor authentication code is:</p><h2>{code}</h2><p>This code will expire in 5 minutes.</p><p>If you didn't request this, please ignore this email.</p><p>Best regards,<br>The HelljonVibe Team</p></body></html>";
        await SendEmailAsync(email, subject, body);
    }

    /// <summary>
    /// Sends a generic email.
    /// </summary>
    /// <param name="email">The recipient email address.</param>
    /// <param name="subject">The email subject.</param>
    /// <param name="message">The email message (HTML).</param>
    public async Task SendEmailAsync(string email, string subject, string message)
    {
        try
        {
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(_settings.FromName ?? "HelljonVibe Identity", _settings.FromAddress));
            mimeMessage.To.Add(new MailboxAddress(email));
            mimeMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = message,
                TextBody = StripHtml(message)
            };
            mimeMessage.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();

            if (_settings.UseSsl)
            {
                await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, true);
            }
            else
            {
                await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, false);
            }

            if (!string.IsNullOrEmpty(_settings.Username) && !string.IsNullOrEmpty(_settings.Password))
            {
                await client.AuthenticateAsync(_settings.Username, _settings.Password);
            }

            await client.SendAsync(mimeMessage);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to send email to {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// Strips HTML tags from a string.
    /// </summary>
    /// <param name="html">The HTML string.</param>
    /// <returns>The plain text string.</returns>
    private static string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html))
            return html;

        var result = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]*>", "");
        return System.Text.RegularExpressions.Regex.Replace(result, "&nbsp;", " ");
    }
}
