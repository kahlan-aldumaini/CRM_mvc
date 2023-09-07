using System.ComponentModel.DataAnnotations;

namespace CRM_mvc.Models.Views.Account
{
    public class LoginViewModel
    {
        [EmailAddress(ErrorMessageResourceType = typeof(Resources.ModelErrorsResource), ErrorMessageResourceName = "IsEmail")]
        [Required(ErrorMessageResourceType = typeof(Resources.ModelErrorsResource), ErrorMessageResourceName = "EmailRequired")]
        public string Email { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.ModelErrorsResource), ErrorMessageResourceName = "PasswordRequired")]
        public string Password { get; set; }

        /*public bool RememberMy { get; set; }*/

        public string? ReturnUrl { get; set; }
    }
}
