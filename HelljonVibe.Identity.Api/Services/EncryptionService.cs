using HelljonVibe.Identity.Api.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Security.Cryptography;
using System.Text;

namespace HelljonVibe.Identity.Api.Services;

/// <summary>
/// Implementation of encryption and decryption operations using AES-256-CBC.
/// </summary>
public class EncryptionService : IEncryptionService {
    private readonly EncryptionSettings _settings;
    private readonly byte[] _masterKey;
    private readonly int _iterationCount;
    private readonly int _saltSize;
    private readonly int _ivSize;

    /// <summary>
    /// Initializes a new instance of the <see cref="EncryptionService"/> class.
    /// </summary>
    /// <param name="settings">The encryption settings.</param>
    public EncryptionService(IOptions<EncryptionSettings> settings) {
        _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
        
        // Validate and normalize master key
        if (string.IsNullOrWhiteSpace(_settings.MasterKey)) {
            throw new InvalidOperationException("Master encryption key is not configured.");
        }
        
        // Ensure key is 32 bytes (256 bits) for AES-256
        var keyBytes = Encoding.UTF8.GetBytes(_settings.MasterKey);
        _masterKey = new byte[32];
        Array.Copy(keyBytes, _masterKey, Math.Min(keyBytes.Length, 32));
        
        _iterationCount = _settings.IterationCount;
        _saltSize = _settings.SaltSize;
        _ivSize = _settings.IvSize;
    }

    /// <summary>
    /// Encrypts a plain text string using AES-256-CBC.
    /// </summary>
    /// <param name="plainText">The plain text to encrypt.</param>
    /// <returns>The encrypted string in format: IV:Salt:CipherText (Base64 encoded).</returns>
    public string Encrypt(string plainText) {
        if (string.IsNullOrEmpty(plainText)) {
            return plainText;
        }

        // Generate random salt and IV
        var salt = new byte[_saltSize];
        var iv = new byte[_ivSize];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        rng.GetBytes(iv);

        // Derive key using PBKDF2
        using var keyDerivation = new Rfc2898DeriveBytes(_masterKey, salt, _iterationCount);
        var key = keyDerivation.GetBytes(32);

        // Encrypt
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new System.IO.MemoryStream();
        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        using var sw = new System.IO.StreamWriter(cs);
        sw.Write(plainText);
        sw.Flush();
        cs.FlushFinalBlock();

        var cipherText = ms.ToArray();

        // Combine IV, salt, and cipher text
        var result = new byte[_ivSize + _saltSize + cipherText.Length];
        Array.Copy(iv, 0, result, 0, _ivSize);
        Array.Copy(salt, 0, result, _ivSize, _saltSize);
        Array.Copy(cipherText, 0, result, _ivSize + _saltSize, cipherText.Length);

        return Convert.ToBase64String(result);
    }

    /// <summary>
    /// Decrypts an encrypted string using AES-256-CBC.
    /// </summary>
    /// <param name="encryptedText">The encrypted text to decrypt (format: IV:Salt:CipherText in Base64).</param>
    /// <returns>The decrypted string.</returns>
    public string Decrypt(string encryptedText) {
        if (string.IsNullOrEmpty(encryptedText)) {
            return encryptedText;
        }

        var data = Convert.FromBase64String(encryptedText);
        
        // Extract IV, salt, and cipher text
        var iv = new byte[_ivSize];
        var salt = new byte[_saltSize];
        var cipherText = new byte[data.Length - _ivSize - _saltSize];
        
        Array.Copy(data, 0, iv, 0, _ivSize);
        Array.Copy(data, _ivSize, salt, 0, _saltSize);
        Array.Copy(data, _ivSize + _saltSize, cipherText, 0, cipherText.Length);

        // Derive key using PBKDF2
        using var keyDerivation = new Rfc2898DeriveBytes(_masterKey, salt, _iterationCount);
        var key = keyDerivation.GetBytes(32);

        // Decrypt
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new System.IO.MemoryStream(cipherText);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new System.IO.StreamReader(cs);

        return sr.ReadToEnd();
    }

    /// <summary>
    /// Generates a new encryption key for a user.
    /// </summary>
    /// <returns>A new encryption key (32 bytes, Base64 encoded).</returns>
    public string GenerateUserKey() {
        var key = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(key);
        return Convert.ToBase64String(key);
    }

    /// <summary>
    /// Encrypts a string using a user-specific key.
    /// </summary>
    /// <param name="plainText">The plain text to encrypt.</param>
    /// <param name="userKey">The user-specific encryption key (Base64 encoded).</param>
    /// <returns>The encrypted string.</returns>
    public string EncryptWithUserKey(string plainText, string userKey) {
        if (string.IsNullOrEmpty(plainText)) {
            return plainText;
        }

        var keyBytes = Convert.FromBase64String(userKey);
        var iv = new byte[_ivSize];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(iv);

        using var aes = Aes.Create();
        aes.Key = keyBytes;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new System.IO.MemoryStream();
        ms.Write(iv, 0, _ivSize); // Prepend IV
        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        using var sw = new System.IO.StreamWriter(cs);
        sw.Write(plainText);
        sw.Flush();
        cs.FlushFinalBlock();

        return Convert.ToBase64String(ms.ToArray());
    }

    /// <summary>
    /// Decrypts a string using a user-specific key.
    /// </summary>
    /// <param name="encryptedText">The encrypted text to decrypt.</param>
    /// <param name="userKey">The user-specific encryption key (Base64 encoded).</param>
    /// <returns>The decrypted string.</returns>
    public string DecryptWithUserKey(string encryptedText, string userKey) {
        if (string.IsNullOrEmpty(encryptedText)) {
            return encryptedText;
        }

        var data = Convert.FromBase64String(encryptedText);
        var keyBytes = Convert.FromBase64String(userKey);
        
        // Extract IV (first 16 bytes)
        var iv = new byte[_ivSize];
        var cipherText = new byte[data.Length - _ivSize];
        Array.Copy(data, 0, iv, 0, _ivSize);
        Array.Copy(data, _ivSize, cipherText, 0, cipherText.Length);

        using var aes = Aes.Create();
        aes.Key = keyBytes;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new System.IO.MemoryStream(cipherText);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new System.IO.StreamReader(cs);

        return sr.ReadToEnd();
    }

    /// <summary>
    /// Hashes a password using BCrypt.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>A tuple containing (hash, salt).</returns>
    public (string Hash, string Salt) HashPassword(string password) {
        if (string.IsNullOrEmpty(password)) {
            throw new ArgumentNullException(nameof(password));
        }

        // Use BCrypt for password hashing
        var salt = BCrypt.Net-Next.BCrypt.GenerateSalt();
        var hash = BCrypt.Net-Next.BCrypt.HashPassword(password, salt);
        
        return (hash, salt);
    }

    /// <summary>
    /// Verifies a password against a hash.
    /// </summary>
    /// <param name="password">The password to verify.</param>
    /// <param name="hash">The stored hash.</param>
    /// <param name="salt">The stored salt (not used with BCrypt as it's included in the hash).</param>
    /// <returns>True if the password matches the hash; otherwise, false.</returns>
    public bool VerifyPassword(string password, string hash, string salt) {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash)) {
            return false;
        }

        // BCrypt hash already contains the salt
        return BCrypt.Net-Next.BCrypt.Verify(password, hash);
    }

    /// <summary>
    /// Generates a random salt.
    /// </summary>
    /// <returns>A random salt string.</returns>
    public string GenerateSalt() {
        var salt = new byte[16];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        return Convert.ToBase64String(salt);
    }

    /// <summary>
    /// Generates a random token.
    /// </summary>
    /// <param name="length">The length of the token.</param>
    /// <returns>A random token string.</returns>
    public string GenerateToken(int length = 32) {
        var token = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(token);
        return Convert.ToBase64String(token)
            .Replace("+", "").Replace("/", "").Replace("=", "")
            .Substring(0, length);
    }

    /// <summary>
    /// Generates a secure random string.
    /// </summary>
    /// <param name="length">The length of the string.</param>
    /// <returns>A secure random string.</returns>
    public string GenerateSecureRandomString(int length = 32) {
        const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var result = new char[length];
        using var rng = RandomNumberGenerator.Create();
        
        for (int i = 0; i < length; i++) {
            var randomIndex = rng.GetInt32(validChars.Length);
            result[i] = validChars[randomIndex];
        }
        
        return new string(result);
    }
}
