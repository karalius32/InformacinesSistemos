using InformacinesSistemos.Models.Library;
using Microsoft.AspNetCore.Identity;

namespace InformacinesSistemos.Data
{
    public class ApplicationUser : IdentityUser
    {
        public UserAccount? Profile { get; set; }
    }
}
