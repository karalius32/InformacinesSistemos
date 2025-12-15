using System.ComponentModel.DataAnnotations;

namespace InformacinesSistemos.ViewModels
{
    public class BookCreateViewModel
    {
        [Required(ErrorMessage = "Pavadinimas yra privalomas")]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Identifier { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? PublishDate { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        [Range(1, 20000, ErrorMessage = "Puslapių skaičius turi būti teigiamas")]
        public int? PageCount { get; set; }

        [MaxLength(255)]
        public string? Publisher { get; set; }

        [MaxLength(255)]
        public string? Language { get; set; }

        [MaxLength(255)]
        public string? Format { get; set; }

        [MaxLength(255)]
        public string? CoverUrl { get; set; }

        [MaxLength(255)]
        public string? Keywords { get; set; }

        // Jei bibliotekoje kuriant norima iškart susieti autorių.
        [MaxLength(255)]
        public string? AuthorFirstName { get; set; }

        [MaxLength(255)]
        public string? AuthorLastName { get; set; }

        public string? ReturnUrl { get; set; }
    }
}