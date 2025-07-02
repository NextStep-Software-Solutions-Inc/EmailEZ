using EmailEZ.Client.Clients;
using Microsoft.Extensions.DependencyInjection;

namespace EmailEZ.Client.DependencyInjection
{
    public static class EmailEZClientServiceCollectionExtensions
    {
        public static IServiceCollection AddEmailEZClient(
            this IServiceCollection services,
            string emailApiBaseUrl,
            string emailApiKey)
        {
            services.AddHttpClient("EmailSenderClient", httpClient =>
            {
                httpClient.Timeout = TimeSpan.FromSeconds(30);
            });

            services.AddTransient<IEmailSenderClient>(serviceProvider =>
            {
                var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient("EmailSenderClient");
                return new EmailSenderClient(httpClient, emailApiBaseUrl, emailApiKey);
            });

            return services;
        }
    }
}
