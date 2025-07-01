using System.Text;
using Carter;
using EmailEZ.Api.Filters;
using EmailEZ.Application;
using EmailEZ.Infrastructure;
using EmailEZ.Infrastructure.Authentication;
using EmailEZ.Infrastructure.Persistence.DbContexts;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
builder.Services.AddAuthentication(options =>
{
    // Set a default scheme if you expect most endpoints to use one more often.
    // If omitted, you MUST explicitly specify schemes on ALL protected endpoints.
    // If present, endpoints without explicit schemes will try this default.
    options.DefaultAuthenticateScheme = ApiKeyAuthenticationOptions.DefaultScheme;
    options.DefaultChallengeScheme = ApiKeyAuthenticationOptions.DefaultScheme;
})
.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
    ApiKeyAuthenticationOptions.DefaultScheme, // Scheme Name: "ApiKey" (from ApiKeyAuthenticationOptions.DefaultScheme)
    null // Display name
)
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options => // Scheme Name: "Bearer" (from JwtBearerDefaults.AuthenticationScheme)
{
    var configuration = builder.Configuration;
    // JWT Bearer Token validation parameters
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = configuration["JwtSettings:Issuer"],
        ValidAudience = configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"] ?? throw new InvalidOperationException("JWT Key not configured.")))
    };
});

builder.Services.AddAuthorization();
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