using System.ComponentModel.DataAnnotations;

namespace InformacinesSistemos.ViewModels
{
    public class EditProfileViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Phone]
        public string? PhoneNumber { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? BirthDate { get; set; }

        public string? Address { get; set; }
    }
}
