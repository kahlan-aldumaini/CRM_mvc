namespace CRM_mvc.Models.Entities;


public class Customer : IBaseModel
{
    public int Id { get; set; }

    public string? Name { get; set; }
    public string PhoneNumber { get; set; }
    public string? Email { get; set; }


    public ICollection<CustomerVerification> CustomerVerifications { get; set; }
    public ICollection<Call> Calls { get; set; }
    public ICollection<Chat> Chats { get; set; }
    public ICollection<Project> Projects { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}