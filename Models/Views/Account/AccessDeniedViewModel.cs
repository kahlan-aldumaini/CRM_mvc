using System.ComponentModel.DataAnnotations;

namespace CRM_mvc.Models.Views.Account
{
    public class AccessDeniedViewModel
    {
        public int? Code { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        [Required]
        public string ReturnUrl { get; set; }

    }
}
