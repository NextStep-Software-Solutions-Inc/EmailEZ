using MediatR;
using EmailEZ.Application.Common.Models; // For PaginatedList
using EmailEZ.Domain.Enums;
using EmailEZ.Application.Features.Emails.Dtos;
using EmailEZ.Application.Common; // For EmailStatus

namespace EmailEZ.Application.Features.Emails.Queries.GetAllEmails;

public record GetAllEmailsQuery() : IRequest<PaginatedList<EmailDto>>
{
    public Guid TenantId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10; // Default page size
    public string SortOrder { get; set; } = SortOder.Descending;

    // Optional filters
    public EmailStatus? Status { get; set; } // Filter by email status
    public string? ToEmailContains { get; set; } // Search for partial email address
    public string? SubjectContains { get; set; } // Search for partial subject
    public DateTimeOffset? StartDate { get; set; } // Filter by queued date range
    public DateTimeOffset? EndDate { get; set; }
}



