namespace HelljonVibe.Identity.Api.Configuration;

/// <summary>
/// Encryption configuration settings.
/// </summary>
public class EncryptionSettings {
    /// <summary>
    /// Gets or sets the master encryption key.
    /// This should be a 32-byte (256-bit) key for AES-256.
    /// </summary>
    public string? MasterKey { get; set; }

    /// <summary>
    /// Gets or sets the encryption algorithm.
    /// Default: AES-256-CBC
    /// </summary>
    public string Algorithm { get; set; } = "AES-256-CBC";

    /// <summary>
    /// Gets or sets the key derivation iteration count.
    /// </summary>
    public int IterationCount { get; set; } = 10000;

    /// <summary>
    /// Gets or sets the salt size in bytes.
    /// </summary>
    public int SaltSize { get; set; } = 16;

    /// <summary>
    /// Gets or sets the initialization vector size in bytes.
    /// </summary>
    public int IvSize { get; set; } = 16;
}
