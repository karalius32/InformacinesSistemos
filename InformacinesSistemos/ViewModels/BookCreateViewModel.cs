using System.ComponentModel.DataAnnotations;
using InformacinesSistemos.Models.Enums;

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

        [MaxLength(255)]
        public string? AuthorFirstName { get; set; }

        [MaxLength(255)]
        public string? AuthorLastName { get; set; }

        public string? ReturnUrl { get; set; }
        public class AuthorListItem
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
        }

        public class SelectedAuthorInput
        {
            public int AuthorId { get; set; }
            public string? Contribution { get; set; }
        }
        public List<AuthorPickRow> AvailableAuthors { get; set; } = new();
        public class AuthorPickRow
        {
            public int AuthorId { get; set; }
            public string FullName { get; set; } = "";
            public bool Selected { get; set; }
            public AuthorRole Role { get; set; } = AuthorRole.Author;

            [Range(0, 100, ErrorMessage = "Indėlis turi būti nuo 0 iki 100")]
            public int? Contribution { get; set; } = 100;
        }
        public int? SelectedCategoryId { get; set; }
        public List<CategoryOption> CategoryOptions { get; set; } = new();

        public class CategoryOption
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
        }
    }
}