using EmailEZ.Application.Interfaces;
using EmailEZ.Application.Specifications.EmailSpecifications;
using EmailEZ.Application.Specifications.WorkspaceSpecifications;
using EmailEZ.Domain.Entities;
using EmailEZ.Domain.Enums;

namespace EmailEZ.Application.Services;

/// <summary>
/// Service demonstrating the use of specification pattern for complex business queries.
/// </summary>
public class SpecificationBasedQueryService
{
    private readonly IUnitOfWork _unitOfWork;

    public SpecificationBasedQueryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    #region Email Specifications Examples

    /// <summary>
    /// Gets emails using specification pattern for better maintainability.
    /// </summary>
    /// <param name="workspaceId">The workspace ID.</param>
    /// <param name="status">Optional status filter.</param>
    /// <param name="includeDetails">Whether to include related entities.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Filtered emails.</returns>
    public async Task<IEnumerable<Email>> GetEmailsByWorkspaceAndStatusAsync(
        Guid workspaceId,
        EmailStatus? status = null,
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        var specification = new EmailsByWorkspaceAndStatusSpecification(workspaceId, status, includeDetails);
        return await _unitOfWork.Emails.GetWithSpecificationAsync(specification, cancellationToken);
    }

    /// <summary>
    /// Gets failed emails that can be retried using specification.
    /// </summary>
    /// <param name="maxRetries">Maximum retry attempts.</param>
    /// <param name="batchSize">Number of emails to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Retryable emails.</returns>
    public async Task<IEnumerable<Email>> GetRetryableEmailsAsync(
        int maxRetries = 3,
        int batchSize = 100,
        CancellationToken cancellationToken = default)
    {
        var specification = new RetryableEmailsSpecification(maxRetries, batchSize);
        return await _unitOfWork.Emails.GetWithSpecificationAsync(specification, cancellationToken);
    }

    /// <summary>
    /// Gets emails within a date range with pagination using specification.
    /// </summary>
    /// <param name="workspaceId">The workspace ID.</param>
    /// <param name="fromDate">Start date.</param>
    /// <param name="toDate">End date.</param>
    /// <param name="pageNumber">Page number.</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="onlySent">Include only sent emails.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated emails in date range.</returns>
    public async Task<(IEnumerable<Email> Items, int TotalCount)> GetEmailsInDateRangeAsync(
        Guid workspaceId,
        DateTimeOffset fromDate,
        DateTimeOffset toDate,
        int pageNumber = 1,
        int pageSize = 50,
        bool onlySent = false,
        CancellationToken cancellationToken = default)
    {
        var specification = new EmailsInDateRangeSpecification(workspaceId, fromDate, toDate, onlySent);
        return await _unitOfWork.Emails.GetPagedWithSpecificationAsync(
            specification, pageNumber, pageSize, cancellationToken);
    }

    /// <summary>
    /// Gets problematic emails that need attention using specification.
    /// </summary>
    /// <param name="workspaceId">The workspace ID.</param>
    /// <param name="minAttempts">Minimum attempt count to be considered problematic.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Problematic emails with events.</returns>
    public async Task<IEnumerable<Email>> GetProblematicEmailsAsync(
        Guid workspaceId,
        int minAttempts = 2,
        CancellationToken cancellationToken = default)
    {
        var specification = new ProblematicEmailsSpecification(workspaceId, minAttempts);
        return await _unitOfWork.Emails.GetWithSpecificationAsync(specification, cancellationToken);
    }

    /// <summary>
    /// Gets emails with large attachments using specification.
    /// </summary>
    /// <param name="workspaceId">The workspace ID.</param>
    /// <param name="minSizeKB">Minimum attachment size in KB.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Emails with large attachments.</returns>
    public async Task<IEnumerable<Email>> GetLargeEmailsAsync(
        Guid workspaceId,
        int minSizeKB = 1000,
        CancellationToken cancellationToken = default)
    {
        var specification = new LargeEmailsSpecification(workspaceId, minSizeKB);
        return await _unitOfWork.Emails.GetWithSpecificationAsync(specification, cancellationToken);
    }

    #endregion

    #region Workspace Specifications Examples

    /// <summary>
    /// Gets active workspaces by domain pattern using specification.
    /// </summary>
    /// <param name="domainPattern">Domain pattern to search for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Active workspaces matching domain pattern.</returns>
    public async Task<IEnumerable<Workspace>> GetActiveWorkspacesByDomainAsync(
        string domainPattern,
        CancellationToken cancellationToken = default)
    {
        var specification = new ActiveWorkspacesByDomainSpecification(domainPattern);
        return await _unitOfWork.Workspaces.GetWithSpecificationAsync(specification, cancellationToken);
    }

    /// <summary>
    /// Gets recently used workspaces using specification.
    /// </summary>
    /// <param name="withinDays">Number of days to look back.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Recently used workspaces.</returns>
    public async Task<IEnumerable<Workspace>> GetRecentlyUsedWorkspacesAsync(
        int withinDays = 30,
        CancellationToken cancellationToken = default)
    {
        var specification = new RecentlyUsedWorkspacesSpecification(TimeSpan.FromDays(withinDays));
        return await _unitOfWork.Workspaces.GetWithSpecificationAsync(specification, cancellationToken);
    }

    /// <summary>
    /// Gets workspaces with their users using specification.
    /// </summary>
    /// <param name="includeUserDetails">Whether to include user details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Workspaces with user information.</returns>
    public async Task<IEnumerable<Workspace>> GetWorkspacesWithUsersAsync(
        bool includeUserDetails = true,
        CancellationToken cancellationToken = default)
    {
        var specification = new WorkspacesWithUsersSpecification(includeUserDetails);
        return await _unitOfWork.Workspaces.GetWithSpecificationAsync(specification, cancellationToken);
    }

    /// <summary>
    /// Gets workspace users by role using specification.
    /// </summary>
    /// <param name="workspaceId">The workspace ID.</param>
    /// <param name="role">The role to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Workspace users with specified role.</returns>
    public async Task<IEnumerable<WorkspaceUser>> GetWorkspaceUsersByRoleAsync(
        Guid workspaceId,
        WorkspaceUserRole role,
        CancellationToken cancellationToken = default)
    {
        var specification = new WorkspaceUsersByRoleSpecification(workspaceId, role);
        return await _unitOfWork.WorkspaceUsers.GetWithSpecificationAsync(specification, cancellationToken);
    }

    /// <summary>
    /// Gets workspace owners using specification.
    /// </summary>
    /// <param name="userId">Optional user ID to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Workspace owners.</returns>
    public async Task<IEnumerable<WorkspaceUser>> GetWorkspaceOwnersAsync(
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        var specification = new WorkspaceOwnersSpecification(userId);
        return await _unitOfWork.WorkspaceUsers.GetWithSpecificationAsync(specification, cancellationToken);
    }

    #endregion

    #region Specification Composition Examples

    /// <summary>
    /// Demonstrates combining multiple specifications for complex business logic.
    /// </summary>
    /// <param name="workspaceId">The workspace ID.</param>
    /// <param name="fromDate">Start date for analysis.</param>
    /// <param name="toDate">End date for analysis.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Comprehensive workspace analysis.</returns>
    public async Task<object> GetWorkspaceAnalysisAsync(
        Guid workspaceId,
        DateTimeOffset fromDate,
        DateTimeOffset toDate,
        CancellationToken cancellationToken = default)
    {
        // Get total emails in period
        var emailsSpec = new EmailsInDateRangeSpecification(workspaceId, fromDate, toDate);
        var totalEmails = await _unitOfWork.Emails.CountWithSpecificationAsync(emailsSpec, cancellationToken);

        // Get sent emails in period
        var sentEmailsSpec = new EmailsInDateRangeSpecification(workspaceId, fromDate, toDate, true);
        var sentEmails = await _unitOfWork.Emails.CountWithSpecificationAsync(sentEmailsSpec, cancellationToken);

        // Get problematic emails
        var problematicSpec = new ProblematicEmailsSpecification(workspaceId);
        var problematicEmails = await _unitOfWork.Emails.CountWithSpecificationAsync(problematicSpec, cancellationToken);

        // Get large emails
        var largeEmailsSpec = new LargeEmailsSpecification(workspaceId);
        var largeEmails = await _unitOfWork.Emails.CountWithSpecificationAsync(largeEmailsSpec, cancellationToken);

        // Get workspace users
        var ownersSpec = new WorkspaceUsersByRoleSpecification(workspaceId, WorkspaceUserRole.Owner);
        var owners = await _unitOfWork.WorkspaceUsers.CountWithSpecificationAsync(ownersSpec, cancellationToken);

        var adminsSpec = new WorkspaceUsersByRoleSpecification(workspaceId, WorkspaceUserRole.Admin);
        var admins = await _unitOfWork.WorkspaceUsers.CountWithSpecificationAsync(adminsSpec, cancellationToken);

        var membersSpec = new WorkspaceUsersByRoleSpecification(workspaceId, WorkspaceUserRole.Member);
        var members = await _unitOfWork.WorkspaceUsers.CountWithSpecificationAsync(membersSpec, cancellationToken);

        return new
        {
            Period = new { FromDate = fromDate, ToDate = toDate },
            EmailMetrics = new
            {
                TotalEmails = totalEmails,
                SentEmails = sentEmails,
                SuccessRate = totalEmails > 0 ? (decimal)sentEmails / totalEmails * 100 : 0,
                ProblematicEmails = problematicEmails,
                LargeEmails = largeEmails
            },
            UserMetrics = new
            {
                Owners = owners,
                Admins = admins,
                Members = members,
                TotalUsers = owners + admins + members
            }
        };
    }

    #endregion
}