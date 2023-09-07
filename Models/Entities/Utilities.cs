namespace CRM_mvc.Models.Entities;

public class Extension : IBaseModel
{
    public int Id { get; set; }

    public int ExtensionNumber { get; set; }

    public ICollection<ApplicationUser> Users { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}