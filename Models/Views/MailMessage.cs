using System.Net.Mail;

namespace CRM_mvc.Models.Views;

public class MailMessage
{
    public MailAddress From { get; set; }
    public List<MailAddress> To { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public bool IsBodyHtml { get; set; }
}