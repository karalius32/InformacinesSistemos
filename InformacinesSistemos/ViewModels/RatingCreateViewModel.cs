using System.ComponentModel.DataAnnotations;

namespace InformacinesSistemos.ViewModels
{
    public class RatingCreateViewModel
    {
        [Required]
        public int BookId { get; set; }

        public string? BookTitle { get; set; }
        public string? AuthorsDisplay { get; set; }

        [Required(ErrorMessage = "Pasirinkite įvertinimą.")]
        [Range(1, 5, ErrorMessage = "Įvertinimas turi būti nuo 1 iki 5.")]
        public int RatingValue { get; set; } = 5;

        [MaxLength(255, ErrorMessage = "Komentaras per ilgas (max 255).")]
        public string? Comment { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
