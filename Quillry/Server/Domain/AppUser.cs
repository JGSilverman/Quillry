using Microsoft.AspNetCore.Identity;

namespace Quillry.Server.Domain
{
    public class AppUser : IdentityUser
    {
        public string DisplayName { get; set; }
        public DateTime JoinedOn { get; set; } = DateTime.Now;
        public bool TermsAgreedTo { get; set; }
        public DateTime TermsAgreedToOn { get; set; }
        public DateTime PasswordLastChanged { get; set; }

        public List<AppUserLogin> Logins { get; set; } = new List<AppUserLogin>();
    }
}
