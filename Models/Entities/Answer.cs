namespace CRM_mvc.Models.Entities;

public class Answer : IBaseModel
{
    public int Id { get; set; }
    public string? Title { get; set; }
    
    public ICollection<ReturnAction> ReturnActions { get; set; }

    public Question Question { get; set; }
    public ApplicationUser User { get; set; }

    public ICollection<Call> Calls { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}