namespace CRM_mvc.Models.Entities;

public class Question : IBaseModel
{
    public int Id { get; set; }
    public string Title { get; set; }
    public ICollection<Answer> Answers { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}