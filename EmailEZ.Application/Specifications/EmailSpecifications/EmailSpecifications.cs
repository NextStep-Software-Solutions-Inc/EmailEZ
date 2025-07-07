using EmailEZ.Application.Specifications;
using EmailEZ.Domain.Entities;
using EmailEZ.Domain.Enums;

namespace EmailEZ.Application.Specifications.EmailSpecifications;

/// <summary>
/// Specification for filtering emails by workspace and status.
/// </summary>
public class EmailsByWorkspaceAndStatusSpecification : BaseSpecification<Email>
{
    public EmailsByWorkspaceAndStatusSpecification(Guid workspaceId, EmailStatus? status = null, bool includeDetails = false)
    {
        if (status.HasValue)
        {
            AddCriteria(e => e.WorkspaceId == workspaceId && e.Status == status.Value);
        }
        else
        {
            AddCriteria(e => e.WorkspaceId == workspaceId);
        }

        if (includeDetails)
        {
            AddInclude(e => e.Attachments);
            AddInclude(e => e.Events);
            AddInclude(e => e.EmailConfiguration);
        }

        AddOrderByDescending(e => e.QueuedAt);
    }
}

/// <summary>
/// Specification for finding failed emails that can be retried.
/// </summary>
public class RetryableEmailsSpecification : BaseSpecification<Email>
{
    public RetryableEmailsSpecification(int maxRetries = 3, int take = 100)
    {
        AddCriteria(e => e.Status == EmailStatus.Failed && e.AttemptCount < maxRetries);
        AddOrderBy(e => e.QueuedAt);
        ApplyPaging(0, take);
    }
}

/// <summary>
/// Specification for emails within a date range with performance metrics.
/// </summary>
public class EmailsInDateRangeSpecification : BaseSpecification<Email>
{
    public EmailsInDateRangeSpecification(
        Guid workspaceId, 
        DateTimeOffset fromDate, 
        DateTimeOffset toDate,
        bool includeOnlySent = false)
    {
        if (includeOnlySent)
        {
            AddCriteria(e => e.WorkspaceId == workspaceId && 
                           e.QueuedAt >= fromDate && 
                           e.QueuedAt <= toDate &&
                           e.Status == EmailStatus.Sent);
        }
        else
        {
            AddCriteria(e => e.WorkspaceId == workspaceId && 
                           e.QueuedAt >= fromDate && 
                           e.QueuedAt <= toDate);
        }

        AddOrderByDescending(e => e.QueuedAt);
    }
}

/// <summary>
/// Specification for emails with high attempt counts (potential issues).
/// </summary>
public class ProblematicEmailsSpecification : BaseSpecification<Email>
{
    public ProblematicEmailsSpecification(Guid workspaceId, int minAttempts = 2)
    {
        AddCriteria(e => e.WorkspaceId == workspaceId && e.AttemptCount >= minAttempts);
        AddInclude(e => e.Events);
        AddOrderByDescending(e => e.AttemptCount);
        AddOrderByDescending(e => e.QueuedAt);
    }
}

/// <summary>
/// Specification for emails with attachments over a certain size.
/// </summary>
public class LargeEmailsSpecification : BaseSpecification<Email>
{
    public LargeEmailsSpecification(Guid workspaceId, int minSizeKB = 1000)
    {
        AddCriteria(e => e.WorkspaceId == workspaceId && 
                        e.Attachments.Any(a => a.FileSizeKB >= minSizeKB));
        
        AddInclude(e => e.Attachments);
        AddOrderByDescending(e => e.Attachments.Sum(a => a.FileSizeKB));
    }
}