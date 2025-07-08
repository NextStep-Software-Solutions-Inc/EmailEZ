using EmailEZ.Application.Interfaces;
using EmailEZ.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmailEZ.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Email entities with custom operations.
/// </summary>
public class EmailConfigurationRepository : GenericRepository<EmailConfiguration>, IEmailConfigurationRepository
{
    public EmailConfigurationRepository(IApplicationDbContext dbContext) : base(dbContext   )
    {
    }

    public async Task<EmailConfiguration?> GetEmailConfigurationByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var emailConfiguration = await _context.EmailConfigurations
            .Where(ec => ec.Id == id)
            .Include(ec => ec.Workspace) // Include Workspace to ensure we can check its status
            .FirstOrDefaultAsync(cancellationToken);

        return emailConfiguration;
    }
}