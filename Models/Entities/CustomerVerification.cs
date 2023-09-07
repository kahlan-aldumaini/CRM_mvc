using System.ComponentModel.DataAnnotations;

namespace CRM_mvc.Models.Entities;

public class CustomerVerification : IBaseModel
{
    public int Id { get; set; }

    public Session? Session { get; set; }
    public Customer Customer { get; set; }

    [Required(ErrorMessage = "يجب إدخال الرقم")]
    [RegularExpression(@"^[0-9]{6}$", ErrorMessage = "يجب إدخال رقم صحيح")]
    [Key]
    public string Code { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}