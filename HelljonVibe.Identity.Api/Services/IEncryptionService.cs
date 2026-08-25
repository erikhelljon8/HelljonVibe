using System;
using System.Threading.Tasks;

namespace HelljonVibe.Identity.Api.Services;

/// <summary>
/// Interface for encryption and decryption operations.
/// </summary>
public interface IEncryptionService {
    /// <summary>
    /// Encrypts a plain text string.
    /// </summary>
    /// <param name="plainText">The plain text to encrypt.</param>
    /// <returns>The encrypted string.</returns>
    string Encrypt(string plainText);

    /// <summary>
    /// Decrypts an encrypted string.
    /// </summary>
    /// <param name="encryptedText">The encrypted text to decrypt.</param>
    /// <returns>The decrypted string.</returns>
    string Decrypt(string encryptedText);

    /// <summary>
    /// Generates a new encryption key for a user.
    /// </summary>
    /// <returns>A new encryption key.</returns>
    string GenerateUserKey();

    /// <summary>
    /// Encrypts a string using a user-specific key.
    /// </summary>
    /// <param name="plainText">The plain text to encrypt.</param>
    /// <param name="userKey">The user-specific encryption key.</param>
    /// <returns>The encrypted string.</returns>
    string EncryptWithUserKey(string plainText, string userKey);

    /// <summary>
    /// Decrypts a string using a user-specific key.
    /// </summary>
    /// <param name="encryptedText">The encrypted text to decrypt.</param>
    /// <param name="userKey">The user-specific encryption key.</param>
    /// <returns>The decrypted string.</returns>
    string DecryptWithUserKey(string encryptedText, string userKey);

    /// <summary>
    /// Hashes a password using BCrypt.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>A tuple containing (hash, salt).</returns>
    (string Hash, string Salt) HashPassword(string password);

    /// <summary>
    /// Verifies a password against a hash.
    /// </summary>
    /// <param name="password">The password to verify.</param>
    /// <param name="hash">The stored hash.</param>
    /// <param name="salt">The stored salt.</param>
    /// <returns>True if the password matches the hash; otherwise, false.</returns>
    bool VerifyPassword(string password, string hash, string salt);

    /// <summary>
    /// Generates a random salt.
    /// </summary>
    /// <returns>A random salt string.</returns>
    string GenerateSalt();

    /// <summary>
    /// Generates a random token.
    /// </summary>
    /// <param name="length">The length of the token.</param>
    /// <returns>A random token string.</returns>
    string GenerateToken(int length = 32);

    /// <summary>
    /// Generates a secure random string.
    /// </summary>
    /// <param name="length">The length of the string.</param>
    /// <returns>A secure random string.</returns>
    string GenerateSecureRandomString(int length = 32);
}
