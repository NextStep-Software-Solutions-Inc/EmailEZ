using EmailEZ.Domain.Entities;
using Microsoft.EntityFrameworkCore; // Required for DbSet<T>

namespace EmailEZ.Application.Interfaces;

/// <summary>
/// Represents the database context for the Email EZ application.
/// This interface defines the contract for data access operations,
/// keeping the Application layer independent of the concrete ORM implementation.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<Email> Emails { get; }
    DbSet<EmailAttachment> EmailAttachments { get; }
    DbSet<EmailEvent> EmailEvents { get; }
    DbSet<AuditLog> AuditLogs { get; }

    DbSet<EmailConfiguration> EmailConfigurations { get; }

    /// <summary>
    /// Saves all changes made in this context to the underlying database.
    /// </summary>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    // You might also add other common methods if needed,
    // e.g., for tracking entities or executing raw SQL.
    // For now, SaveChangesAsync and DbSets are sufficient.
}