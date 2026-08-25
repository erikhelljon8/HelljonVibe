using HelljonVibe.Identity.Models.Entities;

namespace HelljonVibe.Identity.Data.Repositories;

/// <summary>
/// Repository interface for client-specific operations.
/// </summary>
public interface IClientRepository : IRepository<Client> {
    /// <summary>
    /// Gets a client by its client identifier.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The client or null if not found.</returns>
    Task<Client?> GetByClientIdAsync(string clientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a client with the specified client identifier exists.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if a client with the identifier exists; otherwise, false.</returns>
    Task<bool> ClientIdExistsAsync(string clientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active clients.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of active clients.</returns>
    Task<IReadOnlyList<Client>> GetActiveClientsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets clients by their active status.
    /// </summary>
    /// <param name="isActive">The active status.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of clients with the specified active status.</returns>
    Task<IReadOnlyList<Client>> GetByActiveStatusAsync(bool isActive, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets users assigned to a specific client.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of users assigned to the client.</returns>
    Task<IReadOnlyList<User>> GetUsersByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user is assigned to a specific client.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the user is assigned to the client; otherwise, false.</returns>
    Task<bool> IsUserAssignedToClientAsync(Guid userId, Guid clientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a user to a client.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="assignedByUserId">The user identifier of the user who assigned this connection.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created user-client connection.</returns>
    Task<UserClient> AssignUserToClientAsync(Guid userId, Guid clientId, Guid assignedByUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a user from a client.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task RemoveUserFromClientAsync(Guid userId, Guid clientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets clients with pagination.
    /// </summary>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A tuple containing (clients, totalCount).</returns>
    Task<(IReadOnlyList<Client> Clients, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}