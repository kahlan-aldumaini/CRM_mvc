namespace CRM_mvc.Models.Views.Sessions;

public class MessageResponse
{
    public int Id { get; set; }
    public string? Message { get; set; }
    public string DateTime { get; set; }
    public bool FromUser { get; set; }
    public List<MessageAttachmentResponse>? Attachments { get; set; }
}