using System.ComponentModel.DataAnnotations;

namespace CRM_mvc.Models.Views.Message;

public class SendMessageViewModel
{
    [Required(ErrorMessage = "يجب ادخال الرسالة")]
    public string Message { get; set; }
    [Required(ErrorMessage = "يجب ادخال رقم الجوال")]
    [Phone(ErrorMessage = "رقم الجوال غير صحيح")]
    public string Recipient { get; set; }
}