using HelljonVibe.Identity.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace HelljonVibe.Identity.Data.Repositories;

/// <summary>
/// Repository implementation for client-specific operations.
/// </summary>
public class ClientRepository : Repository<Client>, IClientRepository {
    /// <summary>
    /// Initializes a new instance of the <see cref="ClientRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public ClientRepository(ApplicationDbContext context)
        : base(context) {
    }

    /// <summary>
    /// Gets a client by its client identifier.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The client or null if not found.</returns>
    public async Task<Client?> GetByClientIdAsync(string clientId, CancellationToken cancellationToken = default) {
        return await _dbSet.FirstOrDefaultAsync(c => c.ClientId == clientId, cancellationToken);
    }

    /// <summary>
    /// Checks if a client with the specified client identifier exists.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if a client with the identifier exists; otherwise, false.</returns>
    public async Task<bool> ClientIdExistsAsync(string clientId, CancellationToken cancellationToken = default) {
        return await _dbSet.AnyAsync(c => c.ClientId == clientId, cancellationToken);
    }

    /// <summary>
    /// Gets all active clients.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of active clients.</returns>
    public async Task<IReadOnlyList<Client>> GetActiveClientsAsync(CancellationToken cancellationToken = default) {
        return await _dbSet.Where(c => c.IsActive).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets clients by their active status.
    /// </summary>
    /// <param name="isActive">The active status.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of clients with the specified active status.</returns>
    public async Task<IReadOnlyList<Client>> GetByActiveStatusAsync(bool isActive, CancellationToken cancellationToken = default) {
        return await _dbSet.Where(c => c.IsActive == isActive).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets users assigned to a specific client.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of users assigned to the client.</returns>
    public async Task<IReadOnlyList<User>> GetUsersByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default) {
        return await _context.UserClients
            .Where(uc => uc.ClientId == clientId && uc.IsActive)
            .Select(uc => uc.User)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if a user is assigned to a specific client.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the user is assigned to the client; otherwise, false.</returns>
    public async Task<bool> IsUserAssignedToClientAsync(Guid userId, Guid clientId, CancellationToken cancellationToken = default) {
        return await _context.UserClients
            .AnyAsync(uc => uc.UserId == userId && uc.ClientId == clientId && uc.IsActive, cancellationToken);
    }

    /// <summary>
    /// Assigns a user to a client.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="assignedByUserId">The user identifier of the user who assigned this connection.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created user-client connection.</returns>
    public async Task<UserClient> AssignUserToClientAsync(
        Guid userId,
        Guid clientId,
        Guid assignedByUserId,
        CancellationToken cancellationToken = default) {
        var userClient = new UserClient {
            UserId = userId,
            ClientId = clientId,
            AssignedByUserId = assignedByUserId,
            AssignedDate = DateTimeOffset.UtcNow,
            IsActive = true
        };

        await _context.UserClients.AddAsync(userClient, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return userClient;
    }

    /// <summary>
    /// Removes a user from a client.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task RemoveUserFromClientAsync(Guid userId, Guid clientId, CancellationToken cancellationToken = default) {
        var userClient = await _context.UserClients
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ClientId == clientId, cancellationToken);

        if (userClient != null) {
            _context.UserClients.Remove(userClient);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Gets clients with pagination.
    /// </summary>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A tuple containing (clients, totalCount).</returns>
    public async Task<(IReadOnlyList<Client> Clients, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default) {
        var query = _dbSet.OrderBy(c => c.CreatedDate);
        var totalCount = await query.CountAsync(cancellationToken);
        var clients = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (clients, totalCount);
    }
}
