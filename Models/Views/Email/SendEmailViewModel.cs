
using System.ComponentModel.DataAnnotations;
using CRM_mvc.Models.Entities;

namespace CRM_mvc.Models.Views.Email
{
    public class SendEmailViewModel
    {
        public List<Customer>? customers { get; set; }
        public NewSendEmailViewModel NewEmail { get; set; }
    }

    public class NewSendEmailViewModel
    {
        public string? From { get; set; }

        [Required(ErrorMessage = "يجب ادخال على الاقل مستلم واحد")]
        public List<string> To { get; set; }

        [Required(ErrorMessage = "يجب إدخال عنوان الرسالة")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "يجب إدخال نص الرسالة")]
        public string Body { get; set; }
    }
}