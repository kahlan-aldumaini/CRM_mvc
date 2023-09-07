using Microsoft.AspNetCore.Identity;

namespace CRM_mvc.Models.Entities;

public class ApplicationUser : IdentityUser
{
    public string Name { get; set; }

    public ICollection<Extension> Extentions { get; set; }
    public ICollection<Call> Calls { get; set; }
    public ICollection<Answer> Answers { get; set; }
    public ICollection<Session> Sessions { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}