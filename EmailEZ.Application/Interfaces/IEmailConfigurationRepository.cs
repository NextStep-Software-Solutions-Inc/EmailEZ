using EmailEZ.Domain.Entities;

namespace EmailEZ.Application.Interfaces;

public interface IEmailConfigurationRepository : IGenericRepository<EmailConfiguration>
{ 
    Task<EmailConfiguration?> GetEmailConfigurationByIdAsync(Guid id, CancellationToken cancellationToken = default);

}