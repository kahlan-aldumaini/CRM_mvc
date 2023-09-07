namespace CRM_mvc.Models.Entities;

public class CallChannel :IBaseModel
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string Name { get; set; }

    public ICollection<Call> Calls { get; set; }
}