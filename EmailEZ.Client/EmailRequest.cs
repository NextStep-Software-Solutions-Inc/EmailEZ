using Newtonsoft.Json; // We'll install this NuGet package next

namespace EmailEZ.Client
{
    public class EmailRequest
    {
        [JsonProperty("tenantId")]
        public Guid TenantId { get; set; }

        [JsonProperty("emailConfigurationId")]
        public Guid EmailConfigurationId { get; set; }

        [JsonProperty("toEmail")]
        public required List<string> ToEmail { get; set; } = new List<string>();

        [JsonProperty("subject")]
        public required string Subject { get; set; }

        [JsonProperty("body")]
        public required string Body { get; set; }

        [JsonProperty("isHtml")]
        public bool IsHtml { get; set; }

        [JsonProperty("fromDisplayName")]
        public required string FromDisplayName { get; set; }

        [JsonProperty("ccEmail")]
        public List<string> CcEmail { get; set; } = new List<string>();

        [JsonProperty("bccEmail")]
        public List<string> BccEmail { get; set; } = new List<string>();
    }
}