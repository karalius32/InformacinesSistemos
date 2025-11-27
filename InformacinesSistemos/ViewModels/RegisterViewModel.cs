using System.ComponentModel.DataAnnotations;

namespace InformacinesSistemos.ViewModels
{
    public class RegisterViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required, DataType(DataType.Password), Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = "";

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
