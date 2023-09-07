namespace CRM_mvc.Models.Entities;

public class ExtensionUsers : IBaseModel
{
    public int Id { get; set; }

    public int ExtensionId { get; set; }
    public string UsersId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}