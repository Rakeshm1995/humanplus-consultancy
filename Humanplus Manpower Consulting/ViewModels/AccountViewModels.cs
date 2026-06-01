using System.ComponentModel.DataAnnotations;

namespace Humanplus_Manpower_Consulting.ViewModels
{
    public class LoginViewModel
    {
        [Required, Phone, Display(Name = "Mobile Number")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Mobile number must be exactly 10 digits.")]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter a valid 10-digit Indian mobile number starting with 6-9.")]
        public string PhoneNumber { get; set; } = string.Empty;
        [Required, DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
        public string? ReturnUrl { get; set; }
    }

    public class RegisterViewModel
    {
        [Required, Display(Name = "Full Name")] public string FullName { get; set; } = string.Empty;
        [EmailAddress] public string? Email { get; set; }
        [Required, Phone, Display(Name = "Mobile Number")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Mobile number must be exactly 10 digits.")]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter a valid 10-digit Indian mobile number starting with 6-9.")]
        public string PhoneNumber { get; set; } = string.Empty;
        [Required, DataType(DataType.Password), MinLength(6)] public string Password { get; set; } = string.Empty;
        [Required, DataType(DataType.Password), Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
        [Required] public string Role { get; set; } = "JobSeeker";
    }

    public class ForgotPasswordViewModel
    {
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordViewModel
    {
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Required, DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
        [DataType(DataType.Password), Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
