using EmailEZ.Client.Clients;
using EmailEZ.Client.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// --- START: EmailEZ.Client Configuration ---

// 1. Get configuration values
// You would typically store these in appsettings.json or environment variables
var emailApiBaseUrl = builder.Configuration["EmailApi:BaseUrl"];
var emailApiKey = builder.Configuration["EmailApi:ApiKey"];

if (string.IsNullOrEmpty(emailApiBaseUrl) || string.IsNullOrEmpty(emailApiKey))
{
    throw new InvalidOperationException("Email API Base URL and API Key must be configured.");
}

builder.Services.AddEmailEZClient(emailApiBaseUrl, emailApiKey);

// --- END: EmailEZ.Client Configuration ---

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
