namespace CRM_mvc.Models.Entities;

public class ReturnAction
{
    public int Id { get; set; }
    public string Title { get; set; }
    public ICollection<Answer> Answers { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}