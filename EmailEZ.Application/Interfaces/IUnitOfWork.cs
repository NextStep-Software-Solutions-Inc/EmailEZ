using EmailEZ.Domain.Entities;

namespace EmailEZ.Application.Interfaces;

/// <summary>
/// Unit of Work interface that coordinates multiple repositories and provides transaction support.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Gets the repository for Workspace entities.
    /// </summary>
    IGenericRepository<Workspace> Workspaces { get; }

    /// <summary>
    /// Gets the repository for Email entities.
    /// </summary>
    IEmailRepository Emails { get; }

    /// <summary>
    /// Gets the repository for EmailAttachment entities.
    /// </summary>
    IGenericRepository<EmailAttachment> EmailAttachments { get; }

    /// <summary>
    /// Gets the repository for EmailEvent entities.
    /// </summary>
    IGenericRepository<EmailEvent> EmailEvents { get; }

    /// <summary>
    /// Gets the repository for AuditLog entities.
    /// </summary>
    IGenericRepository<AuditLog> AuditLogs { get; }

    /// <summary>
    /// Gets the repository for EmailConfiguration entities.
    /// </summary>
    IEmailConfigurationRepository EmailConfigurations { get; }

    /// <summary>
    /// Gets the specific WorkspaceUser repository with custom operations.
    /// </summary>
    IWorkspaceUserRepository WorkspaceUsers { get; }

    IGenericRepository<WorkspaceApiKey> WorkspaceApiKeys { get; }

    /// <summary>
    /// Saves all changes made in the unit of work to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a database transaction.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}