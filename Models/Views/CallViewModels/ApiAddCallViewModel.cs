using System.ComponentModel.DataAnnotations;

namespace CRM_mvc.Models.Views.CallViewModels;

public class ApiAddCallViewModel
{
    [Required(ErrorMessage = "رقم الهاتف مطلوب")]
    [Phone(ErrorMessage = "يجب ان يكون رقم هاتف صحيح")]
    public string phoneNumber { get; set; }
    
    [Required(ErrorMessage = "رقم الداخلي مطلوب")]
    [Range(100, 999, ErrorMessage = "يجب ان يكون رقم داخلي صحيح")]
    public int Extension { get; set; }
}