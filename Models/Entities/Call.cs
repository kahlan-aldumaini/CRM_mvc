using System.ComponentModel.DataAnnotations;
namespace CRM_mvc.Models.Entities;

public class Call : IBaseModel
{
    public int Id { get; set; }

    [DataType(DataType.Time)]
    public DateTime Start { get; set; }

    [DataType(DataType.Time)]
    public DateTime End { get; set; }

    public string? Note { get; set; }

    // timestamp
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Customer? Customer { get; set; }
    public ApplicationUser User { get; set; }
    public CallType Type { get; set; }
    public CallChannel Channel { get; set; }
    public ICollection<Answer> Answers { get; set; }
    public DateTime? DeletedAt { get; set; }
}