using HelljonVibe.Identity.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace HelljonVibe.Identity.Data;

/// <summary>
/// Unit of work interface for managing database transactions and providing access to repositories.
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable {
    /// <summary>
    /// Gets the user repository.
    /// </summary>
    IUserRepository Users { get; }

    /// <summary>
    /// Gets the client repository.
    /// </summary>
    IClientRepository Clients { get; }

    /// <summary>
    /// Gets the role repository.
    /// </summary>
    IRepository<Models.Entities.Role> Roles { get; }

    /// <summary>
    /// Gets the user role repository.
    /// </summary>
    IRepository<Models.Entities.UserRole> UserRoles { get; }

    /// <summary>
    /// Gets the user client repository.
    /// </summary>
    IRepository<Models.Entities.UserClient> UserClients { get; }

    /// <summary>
    /// Gets the user token repository.
    /// </summary>
    IRepository<Models.Entities.UserToken> UserTokens { get; }

    /// <summary>
    /// Gets the two-factor backup code repository.
    /// </summary>
    IRepository<Models.Entities.TwoFactorBackupCode> TwoFactorBackupCodes { get; }

    /// <summary>
    /// Gets the audit log repository.
    /// </summary>
    IRepository<Models.Entities.AuditLog> AuditLogs { get; }

    /// <summary>
    /// Gets the user consent repository.
    /// </summary>
    IRepository<Models.Entities.UserConsent> UserConsents { get; }

    /// <summary>
    /// Gets the data export request repository.
    /// </summary>
    IRepository<Models.Entities.DataExportRequest> DataExportRequests { get; }

    /// <summary>
    /// Gets the account deletion request repository.
    /// </summary>
    IRepository<Models.Entities.AccountDeletionRequest> AccountDeletionRequests { get; }

    /// <summary>
    /// Commits all changes made within this unit of work.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of affected entries.</returns>
    Task<int> CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back all changes made within this unit of work.
    /// </summary>
    void Rollback();

    /// <summary>
    /// Executes a transaction.
    /// </summary>
    /// <param name="action">The action to execute within the transaction.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task ExecuteTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default);
}