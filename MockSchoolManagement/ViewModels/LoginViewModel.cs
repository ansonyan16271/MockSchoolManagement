using Microsoft.AspNetCore.Authentication;
using System.ComponentModel.DataAnnotations;

namespace MockSchoolManagement.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name ="邮箱")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [Display(Name = "记住我")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }

        // AuthenticationScheme 的命名空间是 Microsoft.AspNetCore.Authentication
        public IList<AuthenticationScheme>? ExternalLogins { get; set; }
    }
}
