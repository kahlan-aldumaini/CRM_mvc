namespace CRM_mvc.Models.Entities;

public class AnswerReturnAction : IBaseModel
{
    public int Id { get; set; }
    public bool IsDone { get; set; }
    public DateTime? DateTime { get; set; }

    public int AnswerId { get; set; }
    public int ReturnActionId { get; set; }


    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}