using EmailEZ.Application.Interfaces;
using EmailEZ.Application.Services;
using EmailEZ.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace EmailEZ.Infrastructure.Extensions;

/// <summary>
/// Extension methods for configuring repository services.
/// </summary>
public static class RepositoryExtensions
{
    /// <summary>
    /// Adds repository services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Register generic repository
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        // Register specific repository interfaces and implementations
        services.AddScoped<IEmailRepository, EmailRepository>();
        services.AddScoped<IWorkspaceUserRepository, WorkspaceUserRepository>();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    /// <summary>
    /// Adds advanced data services that demonstrate complex repository operations.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddAdvancedDataServices(this IServiceCollection services)
    {
        // Register advanced services
        services.AddScoped<IWorkspaceManagementService, WorkspaceManagementService>();
        services.AddScoped<AdvancedDataService>();
        services.AddScoped<SpecificationBasedQueryService>();
        services.AddScoped<SimpleQueryService>();
        services.AddScoped<EmailManagementService>();

        // Note: Business services like WorkspaceManagementService are registered in Application layer
        // Note: EmailManagementService will be registered in Application layer

        return services;
    }

    /// <summary>
    /// Adds all repository and data services in one call.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddDataServices(this IServiceCollection services)
    {
        return services
            .AddRepositories()
            .AddAdvancedDataServices();
    }
}