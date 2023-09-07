namespace CRM_mvc.Models.Entities;

public class ChatSessions: IBaseModel
{
    public int Id { get; set; }
    public Session Session { get; set; }
    public Chat Chat { get; set; }
    public bool IsUser { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}