using EmailEZ.Application.Interfaces; // For IApplicationDbContext, ICurrentUserService, ITenantContext
using EmailEZ.Infrastructure.Persistence.DbContexts;
using EmailEZ.Infrastructure.Services.Security; // For CurrentUserService, TenantContextService
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Hangfire;
using Hangfire.PostgreSql;
using EmailEZ.Infrastructure.Services;

namespace EmailEZ.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions =>
                {
                    // Configure Npgsql specific options if needed
                })
        );

        // Register IApplicationDbContext as an alias for ApplicationDbContext
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // Register Current User Service
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Register Tenant Context Service (often Scoped or Singleton, depending on its use case outside of requests)
        // For per-request tenant context, Scoped is appropriate for API requests.
        services.AddScoped<ITenantContext, TenantContextService>();

        // Register Password Hasher and Encryptor (Singleton as they are stateless)
        services.AddSingleton<IApiKeyHasher, ApiKeyHasher>();


        // Register Email Sender Service
        services.AddTransient<IEmailSender, SmtpEmailSender>(); // Transient for new instance per send

        // Register Hangfire
        services.AddHangfire(config =>
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                  .UseSimpleAssemblyNameTypeSerializer()
                  .UseRecommendedSerializerSettings();

            // Corrected line: Use the options action to specify the Npgsql connection
            config.UsePostgreSqlStorage(options =>
            {
                options.UseNpgsqlConnection(configuration.GetConnectionString("HangfireConnection"));
            });
        });

        services.AddHangfireServer(option =>
        {
            option.SchedulePollingInterval = TimeSpan.FromSeconds(1);
        }); // This adds the background worker processes


        // Read encryption settings from configuration
        var encryptionKey = configuration["EncryptionSettings:Key"];

        // Ensure encryptionKey and encryptionIV are not null or empty
        if (string.IsNullOrWhiteSpace(encryptionKey))
        {
            throw new InvalidOperationException("EncryptionSettings:Ke must be configured in the application settings.");
        }

        services.AddSingleton<IEncryptionService>(new AesEncryptionService(encryptionKey));
        return services;
    }
}