using EmailEZ.Application.Common;
using EmailEZ.Application.Common.Models;
using EmailEZ.Application.Features.Emails.Dtos;
using EmailEZ.Application.Interfaces;
using EmailEZ.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmailEZ.Application.Features.Emails.Queries.GetAllEmails;

public class GetAllEmailsQueryHandler : IRequestHandler<GetAllEmailsQuery, PaginatedList<EmailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllEmailsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<EmailDto>> Handle(GetAllEmailsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Email> query = _context.Emails;

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
            Status = e.Status.ToString(),
            ErrorMessage = e.ErrorMessage,
            QueuedAt = e.QueuedAt,
            SentAt = e.SentAt,
            AttemptCount = e.AttemptCount,
            BodySnippet = e.IsHtml ? e.BodyHtml : e.BodyPlainText, // Select the appropriate body type as snippet
            IsHtml = e.IsHtml
        });

        projectedQuery = request.SortOrder.ToLowerInvariant() switch
        {
            SortOder.Ascending => projectedQuery.OrderBy(e => e.QueuedAt),
            SortOder.Descending => projectedQuery.OrderByDescending(e => e.QueuedAt),
            _ => throw new ArgumentException($"Invalid sort order: {request.SortOrder}. Valid values are 'asc' or 'desc'."),
        };

        return await PaginatedList<EmailDto>.CreateAsync(
            projectedQuery.AsNoTracking(), // Use AsNoTracking for read-only queries
            request.PageNumber,
            request.PageSize
        );
    }
}