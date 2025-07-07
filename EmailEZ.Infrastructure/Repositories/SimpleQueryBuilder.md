# ?? Simple Query Builder

## Overview
The Simple Query Builder provides an easy, fluent way to create dynamic queries without the complexity of specifications.

## ? Simple vs Complex Approach

### ? **Specification Pattern (Complex)**
```csharp
// Need to create a specification class
public class FailedEmailsSpecification : BaseSpecification<Email>
{
    public FailedEmailsSpecification(Guid workspaceId)
    {
        AddCriteria(e => e.WorkspaceId == workspaceId && e.Status == EmailStatus.Failed);
        AddInclude(e => e.Events);
        AddInclude(e => e.Attachments);
        AddOrderByDescending(e => e.QueuedAt);
    }
}

// Use in service
var spec = new FailedEmailsSpecification(workspaceId);
var emails = await _unitOfWork.Emails.GetWithSpecificationAsync(spec);
```

### ? **Query Builder (Simple)**
```csharp
// Direct fluent query - no extra classes needed!
var emails = await _unitOfWork.Emails
    .Query()
    .Where(e => e.WorkspaceId == workspaceId && e.Status == EmailStatus.Failed)
    .Include(e => e.Events)
    .Include(e => e.Attachments)
    .OrderByDescending(e => e.QueuedAt)
    .ToListAsync();
```

## ?? **Key Benefits**

1. **No Extra Classes** - Build queries directly in your service methods
2. **Fluent API** - Chain methods naturally like LINQ
3. **IntelliSense Support** - Full IDE support with auto-completion
4. **Conditional Logic** - Easy to add conditions dynamically
5. **Simple to Learn** - If you know LINQ, you know this pattern

## ?? **Usage Examples**

### Basic Filtering
```csharp
var recentEmails = await _unitOfWork.Emails
    .Query()
    .Where(e => e.WorkspaceId == workspaceId)
    .Where(e => e.QueuedAt >= DateTimeOffset.UtcNow.AddDays(-7))
    .OrderByDescending(e => e.QueuedAt)
    .ToListAsync();
```

### Pagination
```csharp
var (items, totalCount) = await _unitOfWork.Emails
    .Query()
    .Where(e => e.WorkspaceId == workspaceId)
    .OrderBy(e => e.QueuedAt)
    .ToPagedListAsync(pageNumber: 1, pageSize: 20);
```

### Conditional Building
```csharp
var query = _unitOfWork.Emails.Query()
    .Where(e => e.WorkspaceId == workspaceId);

if (status.HasValue)
    query = query.Where(e => e.Status == status.Value);

if (fromDate.HasValue)
    query = query.Where(e => e.QueuedAt >= fromDate.Value);

var results = await query
    .OrderByDescending(e => e.QueuedAt)
    .Take(100)
    .ToListAsync();
```

### Performance Optimizations
```csharp
var readOnlyData = await _unitOfWork.Emails
    .Query()
    .Where(e => e.WorkspaceId == workspaceId)
    .AsNoTracking() // Better performance for read-only
    .Take(1000)
    .ToListAsync();
```

### Getting Single Items
```csharp
var email = await _unitOfWork.Emails
    .Query()
    .Where(e => e.WorkspaceId == workspaceId)
    .Where(e => e.Subject.Contains("Invoice"))
    .OrderByDescending(e => e.QueuedAt)
    .FirstOrDefaultAsync();
```

### Counting Results
```csharp
var count = await _unitOfWork.Emails
    .Query()
    .Where(e => e.WorkspaceId == workspaceId)
    .Where(e => e.Status == EmailStatus.Failed)
    .CountAsync();
```

## ?? **Available Methods**

| Method | Purpose | Example |
|--------|---------|---------|
| `Where()` | Filter entities | `.Where(e => e.Status == EmailStatus.Sent)` |
| `Include()` | Eager load related data | `.Include(e => e.Attachments)` |
| `OrderBy()` | Sort ascending | `.OrderBy(e => e.QueuedAt)` |
| `OrderByDescending()` | Sort descending | `.OrderByDescending(e => e.QueuedAt)` |
| `Skip()` | Skip entities | `.Skip(20)` |
| `Take()` | Limit results | `.Take(10)` |
| `AsNoTracking()` | No change tracking | `.AsNoTracking()` |
| `ToListAsync()` | Execute and get list | `.ToListAsync()` |
| `FirstOrDefaultAsync()` | Get first or null | `.FirstOrDefaultAsync()` |
| `CountAsync()` | Count results | `.CountAsync()` |
| `ToPagedListAsync()` | Paginated results | `.ToPagedListAsync(1, 20)` |

## ?? **Real-World Examples**

### Email Management
```csharp
// Get failed emails that need retry
var retryableEmails = await _unitOfWork.Emails
    .Query()
    .Where(e => e.Status == EmailStatus.Failed)
    .Where(e => e.AttemptCount < 3)
    .OrderBy(e => e.QueuedAt)
    .Take(100)
    .ToListAsync();

// Get large emails with attachments
var largeEmails = await _unitOfWork.Emails
    .Query()
    .Where(e => e.Attachments.Any(a => a.FileSizeKB > 1000))
    .Include(e => e.Attachments)
    .OrderByDescending(e => e.QueuedAt)
    .ToListAsync();
```

### User Management
```csharp
// Get workspace owners
var owners = await _unitOfWork.WorkspaceUsers
    .Query()
    .Where(wu => wu.Role == WorkspaceUserRole.Owner)
    .Include(wu => wu.Workspace)
    .OrderBy(wu => wu.Workspace.Name)
    .ToListAsync();

// Count active users per workspace
var userCount = await _unitOfWork.WorkspaceUsers
    .Query()
    .Where(wu => wu.WorkspaceId == workspaceId)
    .CountAsync();
```

### Reporting Queries
```csharp
// Get data for dashboard (read-only, fast)
var dashboardData = await _unitOfWork.Emails
    .Query()
    .Where(e => e.QueuedAt >= DateTimeOffset.UtcNow.AddDays(-30))
    .AsNoTracking()
    .OrderByDescending(e => e.QueuedAt)
    .ToListAsync();
```

## ? **Performance Tips**

1. **Use `AsNoTracking()`** for read-only queries
2. **Use `Take()`** to limit large result sets
3. **Be selective with `Include()`** - only load what you need
4. **Use pagination** for large datasets with `ToPagedListAsync()`
5. **Filter early** - put `Where()` clauses first

## ?? **Migration from Specifications**

If you have existing specifications, you can easily convert them:

```csharp
// Old specification approach
var spec = new EmailsByWorkspaceSpecification(workspaceId, EmailStatus.Failed);
var emails = await _repository.GetWithSpecificationAsync(spec);

// New query builder approach
var emails = await _repository
    .Query()
    .Where(e => e.WorkspaceId == workspaceId && e.Status == EmailStatus.Failed)
    .ToListAsync();
```

The Query Builder approach is **much simpler** while still being powerful and flexible! ??