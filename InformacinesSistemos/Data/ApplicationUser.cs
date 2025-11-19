using Microsoft.AspNetCore.Identity;

namespace InformacinesSistemos.Data
{
    public class ApplicationUser : IdentityUser
    {
        public UserProfile? Profile { get; set; }
    }
}
