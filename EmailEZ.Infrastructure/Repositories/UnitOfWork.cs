using EmailEZ.Application.Interfaces;
using EmailEZ.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EmailEZ.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation that coordinates multiple repositories and provides transaction support.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly IApplicationDbContext _context;
    private readonly DbContext _dbContext;
    private IDbContextTransaction? _transaction;

    // Repository instances
    private IGenericRepository<Workspace>? _workspaces;
    private IEmailRepository? _emails;
    private IGenericRepository<EmailAttachment>? _emailAttachments;
    private IGenericRepository<EmailEvent>? _emailEvents;
    private IGenericRepository<AuditLog>? _auditLogs;
    private IEmailConfigurationRepository? _emailConfigurations;
    private IWorkspaceUserRepository? _workspaceUsers;

    public UnitOfWork(IApplicationDbContext context)
    {
        _context = context;
        _dbContext = (DbContext)context;
    }

    public IGenericRepository<Workspace> Workspaces
    {
        get
        {
            _workspaces ??= new GenericRepository<Workspace>(_context);
            return _workspaces;
        }
    }

    public IEmailRepository Emails
    {
        get
        {
            _emails ??= new EmailRepository(_context);
            return _emails;
        }
    }

    public IGenericRepository<EmailAttachment> EmailAttachments
    {
        get
        {
            _emailAttachments ??= new GenericRepository<EmailAttachment>(_context);
            return _emailAttachments;
        }
    }

    public IGenericRepository<EmailEvent> EmailEvents
    {
        get
        {
            _emailEvents ??= new GenericRepository<EmailEvent>(_context);
            return _emailEvents;
        }
    }

    public IGenericRepository<AuditLog> AuditLogs
    {
        get
        {
            _auditLogs ??= new GenericRepository<AuditLog>(_context);
            return _auditLogs;
        }
    }

    public IEmailConfigurationRepository EmailConfigurations
    {
        get
        {
            _emailConfigurations ??= new EmailConfigurationRepository(_context);
            return _emailConfigurations;
        }
    }

    public IGenericRepository<WorkspaceUser> WorkspaceUsers
    {
        get
        {
            _workspaceUsers ??= new WorkspaceUserRepository(_context);
            return _workspaceUsers;
        }
    }

    // Specific repository accessors
    public IEmailRepository EmailRepository => (IEmailRepository)Emails;
    public IWorkspaceUserRepository WorkspaceUserRepository => (IWorkspaceUserRepository)WorkspaceUsers;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        GC.SuppressFinalize(this);
    }
}