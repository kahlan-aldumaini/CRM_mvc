using CRM_mvc.Utilities.Enumerations;

namespace CRM_mvc.Models.Entities;

public class MessageAttachment : IBaseModel
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string Path { get; set; }
    public AttachmentType Type { get; set; }
    public Message Message { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}