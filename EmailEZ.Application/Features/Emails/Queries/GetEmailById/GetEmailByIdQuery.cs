using EmailEZ.Application.Features.Emails.Dtos;
using MediatR;

namespace EmailEZ.Application.Features.Emails.Queries.GetEmailById;

public class GetEmailByIdQuery : IRequest<EmailDetailsDto?> // Returns null if not found
{
    public Guid WorkspaceId { get; set; }
    public Guid EmailId { get; set; }
}