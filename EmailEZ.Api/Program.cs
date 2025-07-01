using Carter;
using Clerk.Net.AspNetCore.Security;
using EmailEZ.Api.Filters;
using EmailEZ.Application;
using EmailEZ.Infrastructure;
using EmailEZ.Infrastructure.Authentication;
using EmailEZ.Infrastructure.Persistence.DbContexts;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment.EnvironmentName;

Console.WriteLine($"Environemnt: {env}");

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

// Add services to the container.
builder.Services
    .AddAuthentication(options =>
    {
        // Set your preferred default scheme, or leave unset and specify per endpoint.
        options.DefaultScheme = ApiKeyAuthenticationOptions.DefaultScheme;
        options.DefaultAuthenticateScheme = ApiKeyAuthenticationOptions.DefaultScheme;
        options.DefaultChallengeScheme = ApiKeyAuthenticationOptions.DefaultScheme;
    })
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationOptions.DefaultScheme, // Scheme Name: "ApiKey" (from ApiKeyAuthenticationOptions.DefaultScheme)
        null // Display name
    )
    .AddClerkAuthentication(x =>
        {
            x.Authority = builder.Configuration["Clerk:Authority"]!;
            x.AuthorizedParty = builder.Configuration["Clerk:AuthorizedParty"]!;
        }
    );

builder.Services.AddAuthorization();
builder.Services.AddAuthorizationBuilder().SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Name = "X-API-KEY",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "API Key for tenant authentication"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" }
            },
            new string[] { }
        }
    });
});


builder.Services.AddCarter();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(option =>
    {
        option.ConfigObject.AdditionalItems.Add("persistAuthorization", "true");
        option.ConfigObject.DeepLinking = true;
        option.ConfigObject.DisplayRequestDuration = true;
    });

    // Auto-apply migrations in Development
    // Consider if you want this in production or prefer manual migration application.
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
    }
}

app.UseHttpsRedirection();
//app.UseMiddleware<ApiKeyAuthenticationMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

app.MapCarter();
app.Run();