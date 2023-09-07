namespace CRM_mvc.Models.Entities;

public class Session : IBaseModel
{
    public int Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ICollection<Chat> Chats { get; set; }
    public ApplicationUser? User { get; set; }
    public Customer Customer { get; set; }
    public ICollection<CustomerVerification> customerVerifications { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}