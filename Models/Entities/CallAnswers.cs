namespace CRM_mvc.Models.Entities;

public class CallAnswers : IBaseModel
{
    public int Id { get; set; }
    
    public int CallId { get; set; }
    public int AnswerId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}