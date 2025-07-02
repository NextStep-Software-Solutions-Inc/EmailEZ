using EmailEZ.Client.Clients;
using EmailEZ.Client.Exceptions;
using EmailEZ.Client.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EmailEZ.Client.TestApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // 1. Build the Host (sets up Configuration, Logging, and DI)
            var host = CreateHostBuilder(args).Build();

            // 2. Create a service scope for short-lived services (like in console apps)
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>(); // Get a logger for this Program class

                try
                {
                    // 3. Get the EmailSenderClient instance from the DI container
                    var emailSenderClient = services.GetRequiredService<IEmailSenderClient>();

                    // 4. Prepare your EmailRequest payload
                    var emailRequest = new SendEmailRequest
                    {
                        TenantId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), // REPLACE with your actual Tenant ID
                        EmailConfigurationId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), // REPLACE with your actual Email Configuration ID
                        ToEmail = new List<string> { "test.recipient@example.com" }, // REPLACE with a REAL TEST EMAIL ADDRESS
                        Subject = "Production-Ready Test Email from EmailEZ.Client",
                        Body = "<h1>Hello from the production-ready C# EmailEZ Client!</h1><p>This is a test email sent from a console app using dependency injection and logging.</p>",
                        IsHtml = true,
                        FromDisplayName = "Prod App Tester",
                        CcEmail = new List<string> { "cc.test@example.com" }, // Optional
                        BccEmail = new List<string> { "bcc.test@example.com" } // Optional
                    };

                    logger.LogInformation("Attempting to send email...");

                    // 5. Send the email asynchronously
                    // Update the code to handle the tuple returned by SendEmailAsync
                    var (success, errorMessage) = await emailSenderClient.SendEmailAsync(emailRequest);

                    if (success)
                    {
                        logger.LogInformation("Email sent successfully!");
                    }
                    else
                    {
                        // This path typically means the API returned a non-success status,
                        // but our EmailEZClientException would have been thrown already.
                        // This might be reached if success is explicitly false for other reasons.
                        logger.LogError("Failed to send email (no exception thrown but status not successful).");
                    }
                }
                catch (EmailEZClientException ezEx)
                {
                    // Handle specific API errors from your custom exception
                    logger.LogError(ezEx, "EmailEZ API Error: {Message} (Status: {StatusCode}, Response: {ResponseContent})",
                        ezEx.Message, ezEx.StatusCode, ezEx.ErrorResponse);
                }
                catch (ArgumentException argEx)
                {
                    // Handle input validation errors
                    logger.LogError(argEx, "Input validation error: {Message}", argEx.Message);
                }
                catch (Exception ex)
                {
                    // Catch any other unexpected exceptions
                    logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                }
            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        // Helper method to configure the host (configuration, services, logging)
        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    // Load appsettings.json
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    // Allow environment variables to override settings
                    config.AddEnvironmentVariables();
                    // Allow command line arguments to override settings
                    if (args != null)
                    {
                        config.AddCommandLine(args);
                    }
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // Register EmailSenderClientOptions to be loaded from configuration
                    // IOptionsSnapshot is typically used for options that can change at runtime (e.g., web apps)
                    // For console apps, IOptions (AddOptions().Configure<T>) is also fine.
                    services.Configure<EmailSenderClientOptions>(hostContext.Configuration.GetSection("EmailEZClient"));

                    // Register your IHttpClientFactory and EmailSenderClient
                    // This handles HttpClient lifecycle, pooling, and DI for your client.
                    services.AddHttpClient<IEmailSenderClient, EmailSenderClient>()
                            .ConfigureHttpClient(httpClient =>
                            {
                                // Optional: Configure common HttpClient properties here
                                httpClient.Timeout = TimeSpan.FromSeconds(30); // Example: 30-second timeout
                            })
                            // Optional: Add Polly policies for resilience (retries, circuit breakers)
                            // Requires the "Microsoft.Extensions.Http.Polly" NuGet package
                            // .AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.WaitAndRetryAsync(new[]
                            // {
                            //     TimeSpan.FromSeconds(1),
                            //     TimeSpan.FromSeconds(5),
                            //     TimeSpan.FromSeconds(10)
                            // }));
                            ;
                })
                .ConfigureLogging(logging =>
                {
                    // Clear default providers if you want full control
                    logging.ClearProviders();
                    // Add console logging provider
                    logging.AddConsole();
                    // You can add other logging providers here (e.g., Debug, EventLog, Serilog, NLog, Azure Application Insights)
                });
    }
}