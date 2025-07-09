namespace EmailEZ.Client.Models;

public class SendEmailRequest
{
    public Guid EmailConfigurationId { get; set; }
    public List<string> ToEmail { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public bool IsHtml { get; set; }
    public string FromDisplayName { get; set; }
    public List<string> CcEmail { get; set; }
    public List<string> BccEmail { get; set; }

    public SendEmailRequest()
    {
        ToEmail = new List<string>();
        CcEmail = new List<string>();
        BccEmail = new List<string>();
        Subject = string.Empty; // Initialize to avoid nullability issues
        Body = string.Empty;    // Initialize to avoid nullability issues
        FromDisplayName = string.Empty; // Initialize to avoid nullability issues
    }
}