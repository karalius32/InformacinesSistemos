using InformacinesSistemos.Models;
using Microsoft.AspNetCore.Identity;

namespace InformacinesSistemos.Data
{
    public class ApplicationUser : IdentityUser
    {
        public UserAccount? Profile { get; set; }
    }
}
