namespace CRM_mvc.Models.Entities;

public class Project : IBaseModel
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string Name { get; set; }
    public string Owner { get; set; }
    public string Address { get; set; }
    public ICollection<ProjectType> ProjectTypes { get; set; }
    public Customer Customer { get; set; }
    public ICollection<City> Cities { get; set; }
}