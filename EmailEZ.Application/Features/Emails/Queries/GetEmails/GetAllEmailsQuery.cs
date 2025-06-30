using MediatR;
using EmailEZ.Application.Common.Models; // For PaginatedList
using EmailEZ.Domain.Enums; // For EmailStatus

namespace EmailEZ.Application.Features.Emails.Queries.GetEmails;

public record GetAllEmailsQuery() : IRequest<PaginatedList<EmailDto>>
{
    public Guid TenantId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10; // Default page size

    // Optional filters
    public EmailStatus? Status { get; set; } // Filter by email status
    public string? ToEmailContains { get; set; } // Search for partial email address
    public string? SubjectContains { get; set; } // Search for partial subject
    public DateTimeOffset? StartDate { get; set; } // Filter by queued date range
    public DateTimeOffset? EndDate { get; set; }
}


public record GetEmailsResponse()
{

}
