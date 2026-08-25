using HelljonVibe.Identity.Api.Configuration;
using HelljonVibe.Identity.Data;
using HelljonVibe.Identity.Models.Constants;
using HelljonVibe.Identity.Models.Entities;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace HelljonVibe.Identity.Api.Services;

/// <summary>
/// Implementation of GDPR compliance operations.
/// </summary>
public class GdprService : IGdprService {
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEncryptionService _encryptionService;
    private readonly IEmailService _emailService;
    private readonly AppSettings _appSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="GdprService"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="encryptionService">The encryption service.</param>
    /// <param name="emailService">The email service.</param>
    /// <param name="appSettings">The application settings.</param>
    public GdprService(
        IUnitOfWork unitOfWork,
        IEncryptionService encryptionService,
        IEmailService emailService,
        IOptions<AppSettings> appSettings) {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _appSettings = appSettings.Value ?? throw new ArgumentNullException(nameof(appSettings));
    }

    /// <summary>
    /// Requests data export for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>The data export request.</returns>
    public async Task<DataExportRequest> RequestDataExportAsync(Guid userId) {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) {
            throw new KeyNotFoundException("User not found.");
        }

        // Check for existing active request
        var existingRequest = await _unitOfWork.DataExportRequests
            .GetAllAsync(dr => dr.UserId == userId && dr.Status == DataExportStatus.Pending);
        
        if (existingRequest.Any()) {
            throw new InvalidOperationException("A data export request is already pending.");
        }

        // Create new request
        var request = new DataExportRequest {
            Id = Guid.NewGuid(),
            UserId = userId,
            Status = DataExportStatus.Pending,
            RequestedDate = DateTimeOffset.UtcNow,
            ExpiresDate = DateTimeOffset.UtcNow.AddDays(7),
            DownloadToken = _encryptionService.GenerateToken(64),
            EncryptedFilePath = string.Empty
        };

        await _unitOfWork.DataExportRequests.AddAsync(request);
        await _unitOfWork.CommitAsync();

        // Process the export in background (in production, use a background job)
        _ = ProcessDataExportAsync(request.Id);

        return request;
    }

    /// <summary>
    /// Processes a data export request.
    /// </summary>
    /// <param name="requestId">The request identifier.</param>
    private async Task ProcessDataExportAsync(Guid requestId) {
        try {
            var request = await _unitOfWork.DataExportRequests.GetByIdAsync(requestId);
            if (request == null) {
                return;
            }

            var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
            if (user == null) {
                request.Status = DataExportStatus.Failed;
                request.FailedReason = "User not found";
                await _unitOfWork.DataExportRequests.UpdateAsync(request);
                await _unitOfWork.CommitAsync();
                return;
            }

            // Collect user data
            var exportData = new {
                User = new {
                    Id = user.Id,
                    Username = _encryptionService.Decrypt(user.EncryptedUsername),
                    Email = _encryptionService.Decrypt(user.EncryptedEmail),
                    EmailConfirmed = user.EmailConfirmed,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    IsApproved = user.IsApproved,
                    IsActive = user.IsActive,
                    CreatedDate = user.CreatedDate,
                    LastLoginDate = user.LastLoginDate,
                    PrivacyPolicyAcceptedDate = user.PrivacyPolicyAcceptedDate,
                    TermsAccepted = user.TermsAccepted,
                    TermsAcceptedDate = user.TermsAcceptedDate
                },
                Roles = (await _unitOfWork.UserRoles.GetByUserIdAsync(user.Id))
                    .Select(ur => ur.RoleId)
                    .ToList(),
                Consents = (await _unitOfWork.UserConsents.GetAllAsync(uc => uc.UserId == user.Id))
                    .Select(uc => new {
                        uc.ConsentType,
                        uc.ConsentVersion,
                        uc.IsConsentGiven,
                        uc.ConsentGivenDate
                    })
                    .ToList(),
                AuditLogs = (await _unitOfWork.AuditLogs.GetAllAsync(al => al.UserId == user.Id))
                    .Select(al => new {
                        al.Action,
                        al.EntityType,
                        al.EntityId,
                        al.OldValues,
                        al.NewValues,
                        al.Timestamp,
                        al.IpAddress
                    })
                    .ToList()
            };

            // Serialize to JSON
            var jsonData = JsonSerializer.Serialize(exportData, new JsonSerializerOptions {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            // Encrypt the data
            var encryptedData = _encryptionService.Encrypt(jsonData);

            // Save to file (in production, use secure storage)
            var fileName = $"DataExport_{request.Id}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json.enc";
            var filePath = Path.Combine("DataExports", fileName);
            Directory.CreateDirectory("DataExports");
            await File.WriteAllTextAsync(filePath, encryptedData);

            // Update request
            request.Status = DataExportStatus.Completed;
            request.CompletedDate = DateTimeOffset.UtcNow;
            request.EncryptedFilePath = filePath;
            request.FileSize = encryptedData.Length;

            await _unitOfWork.DataExportRequests.UpdateAsync(request);
            await _unitOfWork.CommitAsync();

            // Send notification email
            var userDecrypted = _encryptionService.Decrypt(user.EncryptedUsername);
            var decryptedEmail = _encryptionService.Decrypt(user.EncryptedEmail);
            var downloadLink = $"{_appSettings.FrontendUrl}/download-data?requestId={request.Id}&token={request.DownloadToken}";
            await _emailService.SendEmailAsync(
                decryptedEmail,
                "Your Data Export is Ready",
                $@"<html><body><h1>Data Export Ready</h1><p>Hello {userDecrypted},</p><p>Your data export is ready for download.</p><p><a href="{downloadLink}">Download Your Data</a></p><p>This link will expire in 7 days.</p><p>Best regards,<br>The HelljonVibe Team</p></body></html>");
        } catch (Exception ex) {
            var request = await _unitOfWork.DataExportRequests.GetByIdAsync(requestId);
            if (request != null) {
                request.Status = DataExportStatus.Failed;
                request.FailedReason = ex.Message;
                await _unitOfWork.DataExportRequests.UpdateAsync(request);
                await _unitOfWork.CommitAsync();
            }
        }
    }

    /// <summary>
    /// Downloads the exported data for a request.
    /// </summary>
    /// <param name="requestId">The request identifier.</param>
    /// <param name="token">The download token.</param>
    /// <returns>The exported data as a byte array.</returns>
    public async Task<byte[]> DownloadDataExportAsync(Guid requestId, string token) {
        var request = await _unitOfWork.DataExportRequests.GetByIdAsync(requestId);
        
        if (request == null || request.Status != DataExportStatus.Completed) {
            throw new InvalidOperationException("Invalid or incomplete request.");
        }

        if (request.DownloadToken != token) {
            throw new UnauthorizedAccessException("Invalid download token.");
        }

        if (request.ExpiresDate < DateTimeOffset.UtcNow) {
            throw new InvalidOperationException("Download link has expired.");
        }

        // Increment download count
        request.DownloadCount++;
        await _unitOfWork.DataExportRequests.UpdateAsync(request);
        await _unitOfWork.CommitAsync();

        // Read and return the file
        if (!File.Exists(request.EncryptedFilePath)) {
            throw new FileNotFoundException("Export file not found.");
        }

        return await File.ReadAllBytesAsync(request.EncryptedFilePath);
    }

    /// <summary>
    /// Requests account deletion for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="reason">The reason for deletion.</param>
    /// <returns>The account deletion request.</returns>
    public async Task<AccountDeletionRequest> RequestAccountDeletionAsync(Guid userId, string reason) {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) {
            throw new KeyNotFoundException("User not found.");
        }

        // Check for existing active request
        var existingRequest = await _unitOfWork.AccountDeletionRequests
            .GetAllAsync(adr => adr.UserId == userId && adr.Status == AccountDeletionStatus.Pending);
        
        if (existingRequest.Any()) {
            throw new InvalidOperationException("An account deletion request is already pending.");
        }

        // Create new request
        var request = new AccountDeletionRequest {
            Id = Guid.NewGuid(),
            UserId = userId,
            Status = AccountDeletionStatus.Pending,
            RequestedDate = DateTimeOffset.UtcNow,
            ConfirmationToken = _encryptionService.GenerateToken(64),
            DeletionReason = reason,
            EncryptedConfirmationToken = _encryptionService.Encrypt(_encryptionService.GenerateToken(64))
        };

        await _unitOfWork.AccountDeletionRequests.AddAsync(request);
        await _unitOfWork.CommitAsync();

        // Send confirmation email
        var decryptedEmail = _encryptionService.Decrypt(user.EncryptedEmail);
        var username = _encryptionService.Decrypt(user.EncryptedUsername);
        var confirmationLink = $"{_appSettings.FrontendUrl}/confirm-deletion?requestId={request.Id}&token={request.ConfirmationToken}";
        
        await _emailService.SendEmailAsync(
            decryptedEmail,
            "Confirm Account Deletion",
            $@"<html><body><h1>Confirm Account Deletion</h1><p>Hello {username},</p><p>We received a request to delete your account. To confirm, please click the link below:</p><p><a href="{confirmationLink}">Confirm Account Deletion</a></p><p>This link will expire in 30 days.</p><p>If you didn't request this, please ignore this email.</p><p>Best regards,<br>The HelljonVibe Team</p></body></html>");

        return request;
    }

    /// <summary>
    /// Confirms an account deletion request.
    /// </summary>
    /// <param name="requestId">The request identifier.</param>
    /// <param name="token">The confirmation token.</param>
    /// <returns>True if the deletion was confirmed successfully.</returns>
    public async Task<bool> ConfirmAccountDeletionAsync(Guid requestId, string token) {
        var request = await _unitOfWork.AccountDeletionRequests.GetByIdAsync(requestId);
        
        if (request == null || request.Status != AccountDeletionStatus.Pending) {
            return false;
        }

        if (request.ConfirmationToken != token) {
            return false;
        }

        // Check if token is expired (30 days)
        if (request.RequestedDate.AddDays(30) < DateTimeOffset.UtcNow) {
            request.Status = AccountDeletionStatus.Expired;
            await _unitOfWork.AccountDeletionRequests.UpdateAsync(request);
            await _unitOfWork.CommitAsync();
            return false;
        }

        // Mark as confirmed and schedule deletion
        request.Status = AccountDeletionStatus.Confirmed;
        request.ConfirmedDate = DateTimeOffset.UtcNow;
        request.DeletionDate = DateTimeOffset.UtcNow.AddDays(14); // GDPR: 14 days grace period
        
        await _unitOfWork.AccountDeletionRequests.UpdateAsync(request);
        await _unitOfWork.CommitAsync();

        // In production, schedule a background job to perform the actual deletion
        _ = ScheduleAccountDeletionAsync(request.UserId, request.DeletionDate.Value);

        return true;
    }

    /// <summary>
    /// Schedules the actual account deletion.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="deletionDate">The deletion date.</param>
    private async Task ScheduleAccountDeletionAsync(Guid userId, DateTimeOffset deletionDate) {
        // Wait until deletion date
        var delay = deletionDate - DateTimeOffset.UtcNow;
        if (delay > TimeSpan.Zero) {
            await Task.Delay(delay);
        }

        // Perform deletion
        await DeleteUserAccountAsync(userId);
    }

    /// <summary>
    /// Deletes a user account and all associated data.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    private async Task DeleteUserAccountAsync(Guid userId) {
        using var transaction = await _unitOfWork.Context.Database.BeginTransactionAsync();
        
        try {
            // Delete all user-related data
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) {
                return;
            }

            // Delete user tokens
            var userTokens = await _unitOfWork.UserTokens.GetAllAsync(ut => ut.UserId == userId);
            foreach (var token in userTokens) {
                await _unitOfWork.UserTokens.DeleteAsync(token.Id);
            }

            // Delete two-factor backup codes
            var backupCodes = await _unitOfWork.TwoFactorBackupCodes.GetAllAsync(tfb => tfb.UserId == userId);
            foreach (var code in backupCodes) {
                await _unitOfWork.TwoFactorBackupCodes.DeleteAsync(code.Id);
            }

            // Delete user consents
            var consents = await _unitOfWork.UserConsents.GetAllAsync(uc => uc.UserId == userId);
            foreach (var consent in consents) {
                await _unitOfWork.UserConsents.DeleteAsync(consent.Id);
            }

            // Delete data export requests
            var exportRequests = await _unitOfWork.DataExportRequests.GetAllAsync(der => der.UserId == userId);
            foreach (var export in exportRequests) {
                await _unitOfWork.DataExportRequests.DeleteAsync(export.Id);
            }

            // Delete account deletion requests
            var deletionRequests = await _unitOfWork.AccountDeletionRequests.GetAllAsync(adr => adr.UserId == userId);
            foreach (var deletion in deletionRequests) {
                await _unitOfWork.AccountDeletionRequests.DeleteAsync(deletion.Id);
            }

            // Delete audit logs for this user
            var auditLogs = await _unitOfWork.AuditLogs.GetAllAsync(al => al.UserId == userId);
            foreach (var log in auditLogs) {
                await _unitOfWork.AuditLogs.DeleteAsync(log.Id);
            }

            // Delete user roles
            var userRoles = await _unitOfWork.UserRoles.GetAllAsync(ur => ur.UserId == userId);
            foreach (var role in userRoles) {
                await _unitOfWork.UserRoles.DeleteAsync(new[] { role.UserId, role.RoleId });
            }

            // Delete user clients
            var userClients = await _unitOfWork.UserClients.GetAllAsync(uc => uc.UserId == userId);
            foreach (var client in userClients) {
                await _unitOfWork.UserClients.DeleteAsync(new[] { client.UserId, client.ClientId });
            }

            // Delete the user
            await _unitOfWork.Users.DeleteAsync(userId);

            await _unitOfWork.CommitAsync();
            await transaction.CommitAsync();

            // Log the deletion
            Log.Information("Account {UserId} deleted successfully as per GDPR request", userId);
        } catch (Exception ex) {
            await transaction.RollbackAsync();
            Log.Error(ex, "Error deleting account {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Updates user consent for GDPR compliance.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="consentType">The consent type.</param>
    /// <param name="consentVersion">The consent version.</param>
    /// <param name="isConsentGiven">Whether consent is given.</param>
    /// <param name="consentDocument">The consent document (optional).</param>
    /// <returns>The user consent record.</returns>
    public async Task<UserConsent> UpdateConsentAsync(
        Guid userId,
        string consentType,
        string consentVersion,
        bool isConsentGiven,
        string? consentDocument = null) {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) {
            throw new KeyNotFoundException("User not found.");
        }

        // Check for existing consent
        var existingConsent = await _unitOfWork.UserConsents
            .GetAllAsync(uc => uc.UserId == userId && uc.ConsentType == consentType);
        
        UserConsent consent;
        if (existingConsent.Any()) {
            consent = existingConsent.First();
            consent.ConsentVersion = consentVersion;
            consent.IsConsentGiven = isConsentGiven;
            consent.ConsentGivenDate = DateTimeOffset.UtcNow;
            consent.LastUpdatedDate = DateTimeOffset.UtcNow;
            
            if (consentDocument != null) {
                consent.EncryptedConsentDocument = _encryptionService.Encrypt(consentDocument);
            }
            
            await _unitOfWork.UserConsents.UpdateAsync(consent);
        } else {
            consent = new UserConsent {
                Id = Guid.NewGuid(),
                UserId = userId,
                ConsentType = consentType,
                ConsentVersion = consentVersion,
                IsConsentGiven = isConsentGiven,
                ConsentGivenDate = DateTimeOffset.UtcNow,
                IsRequired = consentType == GdprConstants.ConsentTypePrivacyPolicy,
                CreatedDate = DateTimeOffset.UtcNow,
                EncryptedConsentDocument = consentDocument != null ? _encryptionService.Encrypt(consentDocument) : null
            };
            
            await _unitOfWork.UserConsents.AddAsync(consent);
        }

        await _unitOfWork.CommitAsync();

        // Update user privacy policy fields if this is privacy policy consent
        if (consentType == GdprConstants.ConsentTypePrivacyPolicy) {
            user.PrivacyPolicyAcceptedDate = isConsentGiven ? DateTimeOffset.UtcNow : null;
            user.PrivacyPolicyVersion = isConsentGiven ? consentVersion : null;
            user.ConsentLastUpdatedDate = DateTimeOffset.UtcNow;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CommitAsync();
        }

        return consent;
    }

    /// <summary>
    /// Gets the consent history for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A list of user consents.</returns>
    public async Task<List<UserConsent>> GetConsentHistoryAsync(Guid userId) {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) {
            throw new KeyNotFoundException("User not found.");
        }

        return await _unitOfWork.UserConsents.GetAllAsync(uc => uc.UserId == userId);
    }

    /// <summary>
    /// Gets the current GDPR compliance status for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>The GDPR compliance status.</returns>
    public async Task<GdprComplianceStatus> GetGdprComplianceStatusAsync(Guid userId) {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) {
            throw new KeyNotFoundException("User not found.");
        }

        var status = new GdprComplianceStatus {
            UserId = userId,
            EmailConfirmed = user.EmailConfirmed,
            PrivacyPolicyAccepted = user.PrivacyPolicyAcceptedDate.HasValue,
            PrivacyPolicyVersion = user.PrivacyPolicyVersion,
            TermsAccepted = user.TermsAccepted,
            TermsVersion = user.TermsVersion,
            ConsentLastUpdated = user.ConsentLastUpdatedDate,
            DataExportRequests = new List<DataExportRequestInfo>(),
            AccountDeletionRequests = new List<AccountDeletionRequestInfo>()
        };

        // Get data export requests
        var exportRequests = await _unitOfWork.DataExportRequests.GetAllAsync(dr => dr.UserId == userId);
        foreach (var request in exportRequests) {
            status.DataExportRequests.Add(new DataExportRequestInfo {
                RequestId = request.Id,
                Status = request.Status.ToString(),
                RequestedDate = request.RequestedDate,
                CompletedDate = request.CompletedDate,
                ExpiresDate = request.ExpiresDate,
                DownloadCount = request.DownloadCount
            });
        }

        // Get account deletion requests
        var deletionRequests = await _unitOfWork.AccountDeletionRequests.GetAllAsync(adr => adr.UserId == userId);
        foreach (var request in deletionRequests) {
            status.AccountDeletionRequests.Add(new AccountDeletionRequestInfo {
                RequestId = request.Id,
                Status = request.Status.ToString(),
                RequestedDate = request.RequestedDate,
                ConfirmedDate = request.ConfirmedDate,
                DeletionDate = request.DeletionDate,
                DeletionReason = request.DeletionReason
            });
        }

        return status;
    }
}

/// <summary>
/// GDPR Compliance Status DTO.
/// </summary>
public class GdprComplianceStatus {
    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the email is confirmed.
    /// </summary>
    public bool EmailConfirmed { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the privacy policy is accepted.
    /// </summary>
    public bool PrivacyPolicyAccepted { get; set; }

    /// <summary>
    /// Gets or sets the privacy policy version.
    /// </summary>
    public string? PrivacyPolicyVersion { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the terms are accepted.
    /// </summary>
    public bool TermsAccepted { get; set; }

    /// <summary>
    /// Gets or sets the terms version.
    /// </summary>
    public string? TermsVersion { get; set; }

    /// <summary>
    /// Gets or sets the consent last updated date.
    /// </summary>
    public DateTimeOffset? ConsentLastUpdated { get; set; }

    /// <summary>
    /// Gets or sets the data export requests.
    /// </summary>
    public List<DataExportRequestInfo> DataExportRequests { get; set; } = new();

    /// <summary>
    /// Gets or sets the account deletion requests.
    /// </summary>
    public List<AccountDeletionRequestInfo> AccountDeletionRequests { get; set; } = new();
}

/// <summary>
/// Data Export Request Information DTO.
/// </summary>
public class DataExportRequestInfo {
    /// <summary>
    /// Gets or sets the request identifier.
    /// </summary>
    public Guid RequestId { get; set; }

    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the requested date.
    /// </summary>
    public DateTimeOffset RequestedDate { get; set; }

    /// <summary>
    /// Gets or sets the completed date.
    /// </summary>
    public DateTimeOffset? CompletedDate { get; set; }

    /// <summary>
    /// Gets or sets the expires date.
    /// </summary>
    public DateTimeOffset ExpiresDate { get; set; }

    /// <summary>
    /// Gets or sets the download count.
    /// </summary>
    public int DownloadCount { get; set; }
}

/// <summary>
/// Account Deletion Request Information DTO.
/// </summary>
public class AccountDeletionRequestInfo {
    /// <summary>
    /// Gets or sets the request identifier.
    /// </summary>
    public Guid RequestId { get; set; }

    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the requested date.
    /// </summary>
    public DateTimeOffset RequestedDate { get; set; }

    /// <summary>
    /// Gets or sets the confirmed date.
    /// </summary>
    public DateTimeOffset? ConfirmedDate { get; set; }

    /// <summary>
    /// Gets or sets the deletion date.
    /// </summary>
    public DateTimeOffset? DeletionDate { get; set; }

    /// <summary>
    /// Gets or sets the deletion reason.
    /// </summary>
    public string? DeletionReason { get; set; }
}

/// <summary>
/// Extensions for IRepository to support GDPR operations.
/// </summary>
public static class GdprRepositoryExtensions {
    /// <summary>
    /// Gets all entities matching a predicate.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="repository">The repository.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns>A list of entities.</returns>
    public static async Task<List<T>> GetAllAsync<T>(this IRepository<T> repository, System.Linq.Expressions.Expression<Func<T, bool>> predicate) where T : class {
        var dbSet = repository.GetDbSet();
        return await dbSet.Where(predicate).ToListAsync();
    }

    /// <summary>
    /// Deletes an entity by composite key.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="repository">The repository.</param>
    /// <param name="keyValues">The key values.</param>
    public static async Task DeleteAsync<T>(this IRepository<T> repository, object[] keyValues) where T : class {
        var entity = await repository.GetByIdAsync(keyValues);
        if (entity != null) {
            await repository.DeleteAsync(entity);
        }
    }
}
