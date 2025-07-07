# Generic Repository Pattern Implementation

## Overview
This implementation provides a comprehensive generic repository pattern in the EmailEZ infrastructure layer, following clean architecture principles and best practices for .NET 8.

## Architecture

### 1. Generic Repository Interface (`IGenericRepository<T>`)
- **Location**: `EmailEZ.Application\Interfaces\IGenericRepository.cs`
- **Purpose**: Defines common CRUD operations for entities inheriting from `BaseEntity`
- **Key Features**:
  - Standard CRUD operations (Create, Read, Update, Delete)
  - Soft delete support
  - Hard delete support
  - Pagination support
  - Include support for related entities
  - Expression-based querying

### 2. Generic Repository Implementation (`GenericRepository<T>`)
- **Location**: `EmailEZ.Infrastructure\Repositories\GenericRepository.cs`
- **Purpose**: Implements the generic repository interface using Entity Framework Core
- **Key Features**:
  - Automatic audit field management through `BaseEntity`
  - Soft delete by setting `IsDeleted = true` and `DeletedAt = DateTimeOffset.UtcNow`
  - Transaction support through `IApplicationDbContext`
  - Pagination with total count calculation
  - Include support for eager loading

### 3. Specific Repository Interfaces
- **IEmailRepository**: Extends `IGenericRepository<Email>` with email-specific operations
- **IWorkspaceUserRepository**: Extends `IGenericRepository<WorkspaceUser>` with role-based operations

### 4. Unit of Work Pattern (`IUnitOfWork`)
- **Location**: `EmailEZ.Application\Interfaces\IUnitOfWork.cs`
- **Purpose**: Coordinates multiple repositories and provides transaction management
- **Key Features**:
  - Access to all repository instances
  - Transaction management (Begin, Commit, Rollback)
  - Centralized SaveChanges operation
  - Disposal pattern implementation

## Key Components

### Repository Operations
#### Generic operations available for all entities
Task<TEntity?> GetByIdAsync(Guid id);
Task<IEnumerable<TEntity>> GetAllAsync();
Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
Task<TEntity> AddAsync(TEntity entity);
Task<TEntity> UpdateAsync(TEntity entity);
Task SoftDeleteAsync(TEntity entity);
Task HardDeleteAsync(TEntity entity);
Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);

#### Specific Repository Operations
##### WorkspaceUser specific operations
Task<WorkspaceUser?> GetByWorkspaceAndUserAsync(Guid workspaceId, string userId);
Task<bool> HasMinimumRoleAsync(Guid workspaceId, string userId, WorkspaceUserRole minimumRole);
Task<WorkspaceUser?> GetOwnerAsync(Guid workspaceId);

##### Email specific operations
Task<IEnumerable<Email>> GetByWorkspaceIdAsync(Guid workspaceId);
Task<IEnumerable<Email>> GetQueuedEmailsAsync(int maxRetries = 3);
Task<Dictionary<EmailStatus, int>> GetEmailStatisticsAsync(Guid workspaceId);

### Transaction Management
#### Using Unit of Work with transactionsawait _unitOfWork.BeginTransactionAsync();
try
{
    var workspace = await _unitOfWork.Workspaces.AddAsync(newWorkspace);
    var workspaceUser = await _unitOfWork.WorkspaceUsers.AddAsync(newUser);
    await _unitOfWork.CommitTransactionAsync();
}
catch
{
    await _unitOfWork.RollbackTransactionAsync();
    throw;
}
## ?? **IMPORTANT: Transaction Management**

### Repository Responsibility
- **Repositories** only modify the `DbContext` change tracker
- **Repositories** do NOT call `SaveChangesAsync()` directly
- **Unit of Work** controls when changes are saved to the database

### Correct Usage Patterns

#### ? **Single Operation (Auto-Save)**public class EmailService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<Email> CreateEmailAsync(Email email)
    {
        var createdEmail = await _unitOfWork.Emails.AddAsync(email);
        await _unitOfWork.SaveChangesAsync(); // Explicitly save
        return createdEmail;
    }
}
#### ? **Multiple Operations with Transaction**public class WorkspaceService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<Workspace> CreateWorkspaceWithOwnerAsync(...)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // Add workspace (NOT saved yet)
            var workspace = await _unitOfWork.Workspaces.AddAsync(newWorkspace);
            
            // Add owner (NOT saved yet)
            var owner = await _unitOfWork.WorkspaceUsers.AddAsync(newOwner);
            
            // Save all changes atomically
            await _unitOfWork.SaveChangesAsync();
            
            // Commit the transaction
            await _unitOfWork.CommitTransactionAsync();
            
            return workspace;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(); // ? Now properly rollbacks
            throw;
        }
    }
}
#### ? **WRONG: Don't use repository directly without Unit of Work**// This won't work properly with transactions
var email = await _emailRepository.AddAsync(newEmail); // Changes are not saved!
### Transaction Flow

1. **Begin Transaction**: `await _unitOfWork.BeginTransactionAsync()`
2. **Repository Operations**: Modify change tracker only
3. **Save Changes**: `await _unitOfWork.SaveChangesAsync()`
4. **Commit**: `await _unitOfWork.CommitTransactionAsync()`
5. **Rollback** (if error): `await _unitOfWork.RollbackTransactionAsync()`

### Key Points

- **Change Tracking**: EF Core tracks all modifications in memory
- **Deferred Execution**: Changes are only written to database when `SaveChangesAsync()` is called
- **Transaction Scope**: All operations within a transaction are atomic
- **Rollback**: Only works if `SaveChangesAsync()` hasn't been called yet

## Usage Examples

### 1. Simple CRUD Operationspublic class EmailService
{
    private readonly IGenericRepository<Email> _emailRepository;
    
    public async Task<Email> CreateEmailAsync(Email email)
    {
        return await _emailRepository.AddAsync(email);
    }
    
    public async Task<Email?> GetEmailAsync(Guid id)
    {
        return await _emailRepository.GetByIdAsync(id);
    }
}
### 2. Using Specific Repository Operationspublic class WorkspaceService
{
    private readonly IWorkspaceUserRepository _workspaceUserRepository;
    
    public async Task<bool> CanUserAccessWorkspaceAsync(Guid workspaceId, string userId)
    {
        return await _workspaceUserRepository.HasMinimumRoleAsync(
            workspaceId, userId, WorkspaceUserRole.Member);
    }
}
### 3. Using Unit of Work for Complex Operationspublic class WorkspaceManagementService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<Workspace> CreateWorkspaceWithOwnerAsync(...)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var workspace = await _unitOfWork.Workspaces.AddAsync(newWorkspace);
            var owner = await _unitOfWork.WorkspaceUsers.AddAsync(newOwner);
            await _unitOfWork.CommitTransactionAsync();
            return workspace;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
## Dependency Injection Configuration

### Registration in Program.cs// Add to your Program.cs or startup configuration
builder.Services.AddRepositories();
### Extension Methodpublic static IServiceCollection AddRepositories(this IServiceCollection services)
{
    services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
    services.AddScoped<IEmailRepository, EmailRepository>();
    services.AddScoped<IWorkspaceUserRepository, WorkspaceUserRepository>();
    services.AddScoped<IUnitOfWork, UnitOfWork>();
    return services;
}
## Benefits

1. **Consistency**: All repositories follow the same pattern and interface
2. **Testability**: Easy to mock interfaces for unit testing
3. **Maintainability**: Centralized data access logic
4. **Flexibility**: Can extend with specific operations per entity
5. **Transaction Support**: Coordinated operations across multiple repositories
6. **Soft Delete**: Built-in support for soft deletion with audit trails
7. **Pagination**: Built-in pagination support for large datasets
8. **Performance**: Include support for efficient data loading

## Best Practices

1. **Always use Unit of Work for multi-repository operations**
2. **Prefer soft delete over hard delete for audit purposes**
3. **Use specific repository interfaces for complex domain-specific operations**
4. **Implement proper transaction handling with try-catch blocks**
5. **Use cancellation tokens for async operations**
6. **Leverage pagination for large datasets**
7. **Use Include operations judiciously to avoid N+1 queries**

## Testing

The repository pattern makes testing easier by providing mockable interfaces:[Test]
public async Task CreateWorkspace_ShouldCreateWorkspaceAndOwner()
{
    // Arrange
    var mockUnitOfWork = new Mock<IUnitOfWork>();
    var service = new WorkspaceManagementService(mockUnitOfWork.Object);
    
    // Act & Assert
    // ... test implementation
}
## ?? **Advanced Repository Methods**

The repository pattern now includes complex but common operations for enterprise applications:

### ?? **Batch Operations**// Batch update emails status
var updateCount = await _unitOfWork.Emails.BatchUpdateAsync(
    e => e.WorkspaceId == workspaceId && e.Status == EmailStatus.Queued,
    e => new Email { Status = EmailStatus.Processing },
    cancellationToken);

// Batch soft delete old records
var deleteCount = await _unitOfWork.Emails.BatchSoftDeleteAsync(
    e => e.QueuedAt < DateTimeOffset.UtcNow.AddDays(-30),
    cancellationToken);

// Upsert operation (insert or update)
var (entity, isInserted) = await _unitOfWork.WorkspaceUsers.UpsertAsync(
    newUser,
    wu => new { wu.WorkspaceId, wu.UserId },
    cancellationToken);
### ?? **Complex Queries**// Advanced query with filtering, ordering, includes, and pagination
var emails = await _unitOfWork.Emails.GetAdvancedAsync(
    filter: e => e.WorkspaceId == workspaceId && e.Status == EmailStatus.Sent,
    orderBy: query => query.OrderByDescending(e => e.QueuedAt),
    includes: new[] { e => e.Attachments, e => e.Events },
    skip: 20,
    take: 10,
    cancellationToken);

// Projection to DTO/ViewModel
var emailSummaries = await _unitOfWork.Emails.ProjectToAsync(
    e => new { e.Id, e.Subject, e.Status, e.QueuedAt },
    e => e.WorkspaceId == workspaceId,
    cancellationToken);

// Group by with aggregation
var statusGroups = await _unitOfWork.Emails.GroupByAsync(
    e => e.Status,
    g => new { Status = g.Key, Count = g.Count() },
    e => e.WorkspaceId == workspaceId,
    cancellationToken);

// Raw SQL execution
var emails = await _unitOfWork.Emails.FromSqlAsync(
    "SELECT * FROM \"Emails\" WHERE \"WorkspaceId\" = {0} AND \"Status\" = {1}",
    new object[] { workspaceId, (int)EmailStatus.Sent },
    cancellationToken);
### ?? **Aggregation Operations**// Statistical operations
var maxAttempts = await _unitOfWork.Emails.MaxAsync(
    e => e.AttemptCount,
    e => e.WorkspaceId == workspaceId,
    cancellationToken);

var avgAttempts = await _unitOfWork.Emails.AverageAsync(
    e => e.AttemptCount,
    e => e.WorkspaceId == workspaceId && e.Status == EmailStatus.Sent,
    cancellationToken);

var totalAttachmentSize = await _unitOfWork.EmailAttachments.SumAsync(
    a => a.FileSizeKB,
    a => a.WorkspaceId == workspaceId,
    cancellationToken);
### ?? **Specification Pattern**
The specification pattern provides a clean way to encapsulate business rules:// Define reusable specifications
public class RetryableEmailsSpecification : BaseSpecification<Email>
{
    public RetryableEmailsSpecification(int maxRetries = 3)
    {
        AddCriteria(e => e.Status == EmailStatus.Failed && e.AttemptCount < maxRetries);
        AddOrderBy(e => e.QueuedAt);
        ApplyPaging(0, 100);
    }
}

// Use specifications in services
var specification = new RetryableEmailsSpecification(maxRetries: 3);
var retryableEmails = await _unitOfWork.Emails.GetWithSpecificationAsync(
    specification, cancellationToken);

// Paginated specification queries
var (items, totalCount) = await _unitOfWork.Emails.GetPagedWithSpecificationAsync(
    specification, pageNumber: 1, pageSize: 50, cancellationToken);
### ??? **Utility Operations**// No-tracking queries for read-only scenarios (better performance)
var readOnlyEmails = await _unitOfWork.Emails.GetAsNoTrackingAsync(
    e => e.WorkspaceId == workspaceId,
    cancellationToken);

// Reload entity from database
await _unitOfWork.Emails.ReloadAsync(emailEntity, cancellationToken);

// Detach entity from context
_unitOfWork.Emails.Detach(emailEntity);
## ?? **Complete Method List**

### Basic CRUD Operations
- `GetByIdAsync()` - Get entity by ID
- `GetAllAsync()` - Get all entities
- `FindAsync()` - Find with predicate
- `FirstOrDefaultAsync()` - Get first matching entity
- `AnyAsync()` - Check existence
- `CountAsync()` - Count entities
- `AddAsync()` - Add single entity
- `AddRangeAsync()` - Add multiple entities
- `UpdateAsync()` - Update entity
- `UpdateRangeAsync()` - Update multiple entities
- `SoftDeleteAsync()` - Soft delete entity
- `HardDeleteAsync()` - Hard delete entity

### Batch Operations
- `BatchUpdateAsync()` - Batch update with expression
- `BatchSoftDeleteAsync()` - Batch soft delete
- `BatchHardDeleteAsync()` - Batch hard delete
- `UpsertAsync()` - Insert or update based on key

### Complex Queries
- `FromSqlAsync()` - Execute raw SQL
- `GetAdvancedAsync()` - Advanced filtering/ordering/includes
- `ProjectToAsync()` - Project to different type
- `GroupByAsync()` - Group and aggregate

### Aggregation Operations
- `MaxAsync()` - Get maximum value
- `MinAsync()` - Get minimum value
- `SumAsync()` - Sum numeric values
- `AverageAsync()` - Calculate average

### Pagination and Includes
- `GetPagedAsync()` - Paginated results
- `GetWithIncludesAsync()` - Eager loading

### Specification Pattern
- `GetWithSpecificationAsync()` - Query with specification
- `GetFirstWithSpecificationAsync()` - Get first with specification
- `CountWithSpecificationAsync()` - Count with specification
- `GetPagedWithSpecificationAsync()` - Paginated specification query

### Utility Operations
- `GetAsNoTrackingAsync()` - No-tracking queries
- `ReloadAsync()` - Reload from database
- `Detach()` - Detach from context

## ?? **Design Patterns Used**
1. **Repository Pattern** - Abstraction over data access
2. **Unit of Work Pattern** - Coordinate multiple repositories
3. **Specification Pattern** - Encapsulate business rules
4. **Generic Pattern** - Type-safe operations for all entities
5. **Fluent Interface** - Easy-to-use API design

## ?? **Performance Considerations**
- **Batch Operations**: Use for bulk updates instead of loops
- **No-Tracking Queries**: Use for read-only scenarios
- **Pagination**: Always paginate large result sets
- **Include Management**: Be selective with eager loading
- **Split Queries**: Use for complex includes (via specifications)
- **Raw SQL**: Use for complex queries that benefit from hand-tuned SQL

## ?? **Usage Examples by Scenario**

### Data Import/Export// Bulk import with upsert
foreach (var batch in importData.Batch(1000))
{
    await _unitOfWork.Emails.AddRangeAsync(batch);
    await _unitOfWork.SaveChangesAsync();
}

// Export with no-tracking for memory efficiency
var exportData = await _unitOfWork.Emails.GetAsNoTrackingAsync(
    e => e.WorkspaceId == workspaceId);
### Reporting and Analytics// Complex reporting query
var report = await _unitOfWork.Emails.GroupByAsync(
    e => new { e.Status, Month = e.QueuedAt.Month },
    g => new { 
        Status = g.Key.Status, 
        Month = g.Key.Month, 
        Count = g.Count(),
        AvgAttempts = g.Average(e => e.AttemptCount)
    },
    e => e.WorkspaceId == workspaceId);
### Background Processing// Get work items for processing
var specification = new RetryableEmailsSpecification(maxRetries: 3);
var emailsToRetry = await _unitOfWork.Emails.GetWithSpecificationAsync(
    specification, cancellationToken);

// Batch update status
await _unitOfWork.Emails.BatchUpdateAsync(
    e => emailsToRetry.Select(x => x.Id).Contains(e.Id),
    e => new Email { Status = EmailStatus.Processing },
    cancellationToken);
This enhanced repository pattern provides enterprise-grade data access capabilities while maintaining clean architecture principles and excellent testability.