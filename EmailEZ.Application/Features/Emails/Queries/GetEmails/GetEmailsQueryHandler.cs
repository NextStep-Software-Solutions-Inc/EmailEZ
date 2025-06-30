using EmailEZ.Application.Common.Models;
using EmailEZ.Application.Interfaces;
using EmailEZ.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmailEZ.Application.Features.Emails.Queries.GetEmails;

public class GetEmailsQueryHandler : IRequestHandler<GetAllEmailsQuery, PaginatedList<EmailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEmailsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<EmailDto>> Handle(GetAllEmailsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Email> query = _context.Emails
            .Where(e => e.TenantId == request.TenantId)
            .OrderByDescending(e => e.QueuedAt); // Default sort order

        // Apply optional filters
        if (request.Status.HasValue)
        {
            query = query.Where(e => e.Status == request.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.ToEmailContains))
        {
            query = query.Where(e => e.ToAddresses.Any(to => to.Contains(request.ToEmailContains)));
        }

        if (!string.IsNullOrWhiteSpace(request.SubjectContains))
        {
            query = query.Where(e => e.Subject.Contains(request.SubjectContains));
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(e => e.QueuedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(e => e.QueuedAt <= request.EndDate.Value);
        }

        // Project to EmailDto before pagination for efficiency
        var projectedQuery = query.Select(e => new EmailDto
        {
            Id = e.Id,
            TenantId = e.TenantId,
            FromAddress = e.FromAddress,
            ToAddresses = e.ToAddresses,
            Subject = e.Subject,
            Status = e.Status,
            ErrorMessage = e.ErrorMessage,
            QueuedAt = e.QueuedAt,
            SentAt = e.SentAt,
            AttemptCount = e.AttemptCount,
            BodySnippet = e.IsHtml ? e.BodyHtml : e.BodyPlainText, // Select the appropriate body type as snippet
            IsHtml = e.BodyHtml != null // Determine if original body was HTML
        });

        return await PaginatedList<EmailDto>.CreateAsync(
            projectedQuery.AsNoTracking(), // Use AsNoTracking for read-only queries
            request.PageNumber,
            request.PageSize
        );
    }
}