using System.Reflection.Emit;
using EmailEZ.Application.Interfaces; // For ICurrentUserService and IApplicationDbContext
using EmailEZ.Domain.Common; // For BaseEntity
using EmailEZ.Domain.Entities; // For our entities
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders; // For EntityTypeBuilder

namespace EmailEZ.Infrastructure.Persistence.DbContexts;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUserService _currentUserService;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
                                ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    // DbSets - must match IApplicationDbContext interface
    public DbSet<Tenant> Tenants { get; set; } = null!;

    public DbSet<EmailConfiguration> EmailConfigurations { get; set; }
    public DbSet<Email> Emails { get; set; } = null!;
    public DbSet<EmailAttachment> EmailAttachments { get; set; } = null!;
    public DbSet<EmailEvent> EmailEvents { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

    public override int SaveChanges()
    {
        ApplyAuditingInformation();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditingInformation();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditingInformation()
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = currentUserId;
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = currentUserId;
                    // If a new entity is added, it's never soft-deleted initially
                    entry.Entity.IsDeleted = false;
                    entry.Entity.DeletedAt = null;
                    entry.Entity.DeletedBy = null;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = currentUserId;

                    // Handle soft-delete transition: if IsDeleted flag was changed from false to true
                    if (entry.OriginalValues.TryGetValue<bool>(nameof(BaseEntity.IsDeleted), out var originalIsDeleted) &&
                        !originalIsDeleted && entry.Entity.IsDeleted)
                    {
                        entry.Entity.DeletedAt = now;
                        entry.Entity.DeletedBy = currentUserId;
                    }
                    // Handle un-delete transition (optional): if IsDeleted flag was changed from true to false
                    else if (originalIsDeleted && !entry.Entity.IsDeleted)
                    {
                        entry.Entity.DeletedAt = null;
                        entry.Entity.DeletedBy = null;
                    }
                    break;

                case EntityState.Deleted:
                    // If you directly call .Remove() on a DbSet, this state is hit.
                    // To enforce soft-delete, we must intercept and change the state to Modified.
                    // This means you should primarily set entity.IsDeleted = true in your application code,
                    // rather than calling DbContext.Remove(entity).
                    if (!entry.Entity.IsDeleted) // Only if not already marked as deleted
                    {
                        entry.Entity.IsDeleted = true;
                        entry.Entity.DeletedAt = now;
                        entry.Entity.DeletedBy = currentUserId;
                        entry.Entity.UpdatedAt = now; // Update timestamp on soft delete
                        entry.Entity.UpdatedBy = currentUserId; // Update by who
                        entry.State = EntityState.Modified; // Change state to Modified to save changes instead of deleting
                    }
                    break;
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Global Query Filter for soft-deletion:
        // This ensures that when you query entities, it automatically filters out those marked as IsDeleted = true.
        // To query deleted entities, use .IgnoreQueryFilters()
        modelBuilder.Entity<Tenant>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Email>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<EmailAttachment>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<EmailEvent>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<AuditLog>().HasQueryFilter(e => !e.IsDeleted);

        // add query filter for tenant id
        modelBuilder.Entity<Email>().HasQueryFilter(e => e.TenantId == _currentUserService.GetCurrentUserId());
        modelBuilder.Entity<EmailAttachment>().HasQueryFilter(e => e.TenantId == _currentUserService.GetCurrentUserId());
        modelBuilder.Entity<EmailEvent>().HasQueryFilter(e => e.TenantId == _currentUserService.GetCurrentUserId());
        modelBuilder.Entity<AuditLog>().HasQueryFilter(e => e.TenantId == _currentUserService.GetCurrentUserId());


        // Call the entity-specific configurations here
        // We can move these into separate configuration classes later for better organization,
        // but for now, keeping them as private methods for clarity within this file.
        ConfigureTenant(modelBuilder.Entity<Tenant>());
        ConfigureEmail(modelBuilder.Entity<Email>());
        ConfigureEmailAttachment(modelBuilder.Entity<EmailAttachment>());
        ConfigureEmailEvent(modelBuilder.Entity<EmailEvent>());
        ConfigureAuditLog(modelBuilder.Entity<AuditLog>());
        ConfigureEmailConfiguration(modelBuilder.Entity<EmailConfiguration>());

        base.OnModelCreating(modelBuilder); // Always call base implementation last
    }

    // Helper methods for organizing Fluent API configurations for each entity
    private void ConfigureTenant(EntityTypeBuilder<Tenant> entity)
    {
        entity.HasKey(t => t.Id); // Ensure primary key is set if not relying on convention
        entity.Property(t => t.Id).ValueGeneratedOnAdd(); // Ensure GUID is generated by DB if needed, or by application (Guid.NewGuid() in BaseEntity handles app-side)

        entity.HasIndex(t => t.ApiKeyHash).IsUnique();
        entity.HasIndex(t => t.Domain).IsUnique();
        entity.Property(t => t.Name).IsRequired().HasMaxLength(255);
        entity.Property(t => t.ApiKeyHash).IsRequired().HasMaxLength(255);
        entity.Property(t => t.Domain).IsRequired().HasMaxLength(255);
    }

    private void ConfigureEmail(EntityTypeBuilder<Email> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).ValueGeneratedOnAdd();

        entity.HasOne(e => e.Tenant)
              .WithMany(t => t.Emails)
              .HasForeignKey(e => e.TenantId)
              .OnDelete(DeleteBehavior.Restrict); // Prevent deleting tenant if emails exist

        entity.Property(e => e.FromAddress).IsRequired().HasMaxLength(255);
        // Npgsql automatically maps List<string> to TEXT[], so no explicit HasColumnType needed for that
        entity.Property(e => e.ToAddresses).IsRequired(); // List<string> cannot be null, but can be empty
        // CcAddresses and BccAddresses are nullable in model, will be TEXT[] null in DB
        entity.Property(e => e.Subject).IsRequired().HasColumnType("text");
        entity.Property(e => e.BodyHtml).HasColumnType("text");
        entity.Property(e => e.BodyHtml).HasMaxLength(2000); // Adjust length as needed
        entity.Property(e => e.BodyPlainText).HasColumnType("text");
        entity.Property(e => e.BodyPlainText).HasMaxLength(2000); // Adjust length as needed

        entity.Property(e => e.Status)
              .HasConversion<string>() // Store enum as string
              .IsRequired()
              .HasMaxLength(50); // Match enum string length

        entity.Property(e => e.ErrorMessage).HasColumnType("text");
        entity.Property(e => e.QueuedAt).IsRequired().HasColumnType("timestamp with time zone");
        entity.Property(e => e.SentAt).HasColumnType("timestamp with time zone");
        entity.Property(e => e.AttemptCount).IsRequired().HasDefaultValue(0);

        entity.HasIndex(e => e.TenantId); // For efficient tenant-specific email queries
        entity.HasIndex(e => e.Status); // For efficient status-based queries
        entity.HasIndex(e => e.QueuedAt); // For processing queue

        entity.Property(e => e.ToAddresses)
            .HasConversion(
                v => string.Join(";", v),
                v => v.Split(";", StringSplitOptions.RemoveEmptyEntries).ToList()
            );
        entity.Property(e => e.CcAddresses)
            .HasConversion(
                v => v != null ? string.Join(";", v) : null,
                v => v != null ? v.Split(";", StringSplitOptions.RemoveEmptyEntries).ToList() : null
            );

        entity.Property(e => e.BccAddresses)
            .HasConversion(
                v => v != null ? string.Join(";", v) : null,
                v => v != null ? v.Split(";", StringSplitOptions.RemoveEmptyEntries).ToList() : null
            );

        entity.HasOne(e => e.EmailConfiguration)
            .WithMany() // Or .WithMany(ec => ec.EmailsSent) if you add a collection to EmailConfiguration
            .HasForeignKey(e => e.EmailConfigurationId)
            .OnDelete(DeleteBehavior.Restrict); // Or .NoAction, Prevent, etc., depending on desired behavior if EmailConfig is deleted
    }

    private void ConfigureEmailAttachment(EntityTypeBuilder<EmailAttachment> entity)
    {
        entity.HasKey(ea => ea.Id);
        entity.Property(ea => ea.Id).ValueGeneratedOnAdd();

        entity.HasOne(ea => ea.Email)
              .WithMany(e => e.Attachments)
              .HasForeignKey(ea => ea.EmailId)
              .OnDelete(DeleteBehavior.Cascade); // Delete attachments if email is deleted

        entity.Property(ea => ea.FileName).IsRequired().HasMaxLength(255);
        entity.Property(ea => ea.ContentType).IsRequired().HasMaxLength(100);
        entity.Property(ea => ea.FileContentBase64).HasColumnType("text"); // Use text for potentially large base64 strings
        entity.Property(ea => ea.FileSizeKB).IsRequired();

        entity.HasIndex(ea => ea.EmailId);
        entity.HasIndex(ea => ea.TenantId); // For denormalized tenant-specific queries
    }

    private void ConfigureEmailEvent(EntityTypeBuilder<EmailEvent> entity)
    {
        entity.HasKey(ee => ee.Id);
        entity.Property(ee => ee.Id).ValueGeneratedOnAdd();

        entity.HasOne(ee => ee.Email)
              .WithMany(e => e.Events)
              .HasForeignKey(ee => ee.EmailId)
              .OnDelete(DeleteBehavior.Cascade); // Delete events if email is deleted

        entity.Property(ee => ee.EventType)
              .HasConversion<string>()
              .IsRequired()
              .HasMaxLength(50); // Match enum string length

        entity.Property(ee => ee.Details).HasColumnType("jsonb"); // For PostgreSQL JSONB type

        entity.HasIndex(ee => ee.EmailId);
        entity.HasIndex(ee => ee.TenantId);
        entity.HasIndex(ee => ee.EventType);
        entity.HasIndex(ee => ee.CreatedAt); // Using BaseEntity.CreatedAt for event timestamp
    }

    private void ConfigureAuditLog(EntityTypeBuilder<AuditLog> entity)
    {
        entity.HasKey(al => al.Id);
        entity.Property(al => al.Id).ValueGeneratedOnAdd();

        entity.HasOne(al => al.Tenant)
              .WithMany(t => t.AuditLogs) // Ensure this navigation property exists on Tenant if you want it
              .HasForeignKey(al => al.TenantId)
              .IsRequired(false) // TenantId is nullable
              .OnDelete(DeleteBehavior.Restrict); // Prevent deleting tenant if audit logs exist

        entity.Property(al => al.EventType)
              .HasConversion<string>()
              .IsRequired()
              .HasMaxLength(100); // Match enum string length

        entity.Property(al => al.PerformedBy).IsRequired().HasMaxLength(255);
        entity.Property(al => al.Details).HasColumnType("jsonb");

        entity.HasIndex(al => al.TenantId);
        entity.HasIndex(al => al.EventType);
        entity.HasIndex(al => al.CreatedAt); // Using BaseEntity.CreatedAt for audit timestamp
    }

    private void ConfigureEmailConfiguration(EntityTypeBuilder<EmailConfiguration> entity) {
        entity.HasKey(e => e.Id); // Define primary key if not covered by BaseAuditableEntity

        // Configure the one-to-many relationship with Tenant
        entity.HasOne(e => e.Tenant)             // An EmailConfiguration has one Tenant
                .WithMany()                        // A Tenant can have many EmailConfigurations (no navigation property on Tenant side)
                .HasForeignKey(e => e.TenantId)    // EmailConfiguration.TenantId is the foreign key
                .IsRequired()                      // An EmailConfiguration must be linked to a Tenant
                .OnDelete(DeleteBehavior.Cascade); // If a Tenant is deleted, delete its EmailConfigurations

        // You might want to make SmtpHost + Username unique per Tenant if only one config per tenant is allowed
        // entity.HasIndex(e => new { e.TenantId, e.SmtpHost, e.Username }).IsUnique();

    }
}