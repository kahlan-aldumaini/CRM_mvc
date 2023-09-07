using System.ComponentModel.DataAnnotations;

namespace CRM_mvc.Models.Views.Sessions;

public class AskForSessionViewModel
{
    [Required(ErrorMessage = "يجب إدخال الإسم")]
    public string Name { get; set; }

    [Required(ErrorMessage = "يجب ادخال رقم التلفون")]
    [Phone(ErrorMessage = "يجب إدخال رقم تلفون صحيح")]
    public string Phone { get; set; }
}