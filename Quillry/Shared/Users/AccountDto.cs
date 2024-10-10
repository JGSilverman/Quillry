using System.ComponentModel.DataAnnotations;

namespace Quillry.Shared
{
    public class UserAccountDto
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public DateTime JoinedOn { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string? PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool TermsAgreedTo { get; set; }
        public DateTime TermsAgreedToOn { get; set; }
        public DateTime PasswordLastChanged { get; set; }
        public bool LockoutEnabled { get; set; }
        public DateTimeOffset LockoutEnd { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password is required.")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Confirm Password is required.")]
        public string ConfirmPassword { get; set; }

        public bool PasswordsMatch
        {
            get
            {
                if (string.IsNullOrEmpty(this.NewPassword)) return false;
                if (string.IsNullOrEmpty(this.ConfirmPassword)) return false;
                return string.Equals(this.NewPassword, this.ConfirmPassword);
            }
        }
    }
}
