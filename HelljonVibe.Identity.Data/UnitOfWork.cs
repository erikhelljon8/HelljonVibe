using HelljonVibe.Identity.Data.Repositories;
using HelljonVibe.Identity.Models.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace HelljonVibe.Identity.Data;

/// <summary>
/// Unit of work implementation for managing database transactions and providing access to repositories.
/// </summary>
public class UnitOfWork : IUnitOfWork {
    /// <summary>
    /// The database context.
    /// </summary>
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public UnitOfWork(ApplicationDbContext context) {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Gets the user repository.
    /// </summary>
    private IUserRepository? _users;
    public IUserRepository Users => _users ??= new UserRepository(_context);

    /// <summary>
    /// Gets the client repository.
    /// </summary>
    private IClientRepository? _clients;
    public IClientRepository Clients => _clients ??= new ClientRepository(_context);

    /// <summary>
    /// Gets the role repository.
    /// </summary>
    private IRepository<Role>? _roles;
    public IRepository<Role> Roles => _roles ??= new Repository<Role>(_context);

    /// <summary>
    /// Gets the user role repository.
    /// </summary>
    private IRepository<UserRole>? _userRoles;
    public IRepository<UserRole> UserRoles => _userRoles ??= new Repository<UserRole>(_context);

    /// <summary>
    /// Gets the user client repository.
    /// </summary>
    private IRepository<UserClient>? _userClients;
    public IRepository<UserClient> UserClients => _userClients ??= new Repository<UserClient>(_context);

    /// <summary>
    /// Gets the user token repository.
    /// </summary>
    private IRepository<UserToken>? _userTokens;
    public IRepository<UserToken> UserTokens => _userTokens ??= new Repository<UserToken>(_context);

    /// <summary>
    /// Gets the two-factor backup code repository.
    /// </summary>
    private IRepository<TwoFactorBackupCode>? _twoFactorBackupCodes;
    public IRepository<TwoFactorBackupCode> TwoFactorBackupCodes =>
        _twoFactorBackupCodes ??= new Repository<TwoFactorBackupCode>(_context);

    /// <summary>
    /// Gets the audit log repository.
    /// </summary>
    private IRepository<AuditLog>? _auditLogs;
    public IRepository<AuditLog> AuditLogs => _auditLogs ??= new Repository<AuditLog>(_context);

    /// <summary>
    /// Gets the user consent repository.
    /// </summary>
    private IRepository<UserConsent>? _userConsents;
    public IRepository<UserConsent> UserConsents => _userConsents ??= new Repository<UserConsent>(_context);

    /// <summary>
    /// Gets the data export request repository.
    /// </summary>
    private IRepository<DataExportRequest>? _dataExportRequests;
    public IRepository<DataExportRequest> DataExportRequests =>
        _dataExportRequests ??= new Repository<DataExportRequest>(_context);

    /// <summary>
    /// Gets the account deletion request repository.
    /// </summary>
    private IRepository<AccountDeletionRequest>? _accountDeletionRequests;
    public IRepository<AccountDeletionRequest> AccountDeletionRequests =>
        _accountDeletionRequests ??= new Repository<AccountDeletionRequest>(_context);

    /// <summary>
    /// Commits all changes made within this unit of work.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of affected entries.</returns>
    public async Task<int> CommitAsync(CancellationToken cancellationToken = default) {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Rolls back all changes made within this unit of work.
    /// </summary>
    public void Rollback() {
        // EF Core does not have an explicit rollback method for the change tracker
        // Changes are automatically rolled back when the context is disposed
        // or when SaveChanges is not called
    }

    /// <summary>
    /// Executes a transaction.
    /// </summary>
    /// <param name="action">The action to execute within the transaction.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task ExecuteTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default) {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try {
            await action();
            await transaction.CommitAsync(cancellationToken);
        } catch {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Disposes the unit of work.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing) {
        if (disposing) {
            _context.Dispose();
        }
    }

    /// <summary>
    /// Disposes the unit of work.
    /// </summary>
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Asynchronously disposes the unit of work.
    /// </summary>
    public async ValueTask DisposeAsync() {
        await _context.DisposeAsync();
        Dispose(false);
        GC.SuppressFinalize(this);
    }
}
