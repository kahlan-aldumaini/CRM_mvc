

using CRM_mvc.Resources.Auth;
using System.ComponentModel.DataAnnotations;

namespace CRM_mvc.Models.Views.Account
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessageResourceType = typeof(ChangePassword), ErrorMessageResourceName = "OldPassword")]
        public string OldPassword { get; set; }

        [Required(ErrorMessageResourceType = typeof(ChangePassword), ErrorMessageResourceName = "NewPassword")]
        public string NewPassword { get; set; }

        [Required(ErrorMessageResourceType = typeof(ChangePassword), ErrorMessageResourceName = "ConfirmPassword")]
        public string ConfirmPassword { get; set; }
    }
}
