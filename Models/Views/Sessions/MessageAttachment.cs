using CRM_mvc.Utilities.Enumerations;

namespace CRM_mvc.Models.Views.Sessions;

public class MessageAttachmentResponse
{
    public string? Path { get; set; }
    public AttachmentType Type { get; set; }
}