namespace HelljonVibe.Identity.Api.Configuration;

/// <summary>
/// Email configuration settings.
/// </summary>
public class EmailSettings {
    /// <summary>
    /// Gets or sets the SMTP host.
    /// </summary>
    public string? SmtpHost { get; set; }

    /// <summary>
    /// Gets or sets the SMTP port.
    /// </summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>
    /// Gets or sets the SMTP username.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the SMTP password.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets the from email address.
    /// </summary>
    public string? FromAddress { get; set; }

    /// <summary>
    /// Gets or sets the from display name.
    /// </summary>
    public string? FromName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use SSL.
    /// </summary>
    public bool UseSsl { get; set; } = true;

    /// <summary>
    /// Gets or sets the email template directory.
    /// </summary>
    public string? TemplateDirectory { get; set; } = "EmailTemplates";

    /// <summary>
    /// Gets or sets the base URL for email links.
    /// </summary>
    public string? BaseUrl { get; set; }
}
