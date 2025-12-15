using System.ComponentModel.DataAnnotations;
using InformacinesSistemos.Models.Enums;

namespace InformacinesSistemos.ViewModels
{
    public class BookEditViewModel
    {
        public int Id { get; set; }

        [MaxLength(255)]
        public string? Title { get; set; }

        [MaxLength(255)]
        public string? Identifier { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? PublishDate { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? UpdatedDate { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        public int? PageCount { get; set; }

        [MaxLength(255)]
        public string? Publisher { get; set; }

        [MaxLength(255)]
        public string? Language { get; set; }

        [MaxLength(255)]
        public string? Format { get; set; }

        public DateTime? CreatedDate { get; set; }

        [MaxLength(255)]
        public string? CoverUrl { get; set; }

        [MaxLength(255)]
        public string? Keywords { get; set; }
        public string? ReturnUrl { get; set; }
        public string? AuthorsDisplay { get; set; }
        public List<AuthorPickRow> AvailableAuthors { get; set; } = new();
        public int? SelectedCategoryId { get; set; }
        public List<CategoryOption> CategoryOptions { get; set; } = new();

        public class CategoryOption
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
        }

        public class AuthorPickRow
        {
            public int AuthorId { get; set; }
            public string FullName { get; set; } = "";
            public bool Selected { get; set; }
            public int? Contribution { get; set; } = 100;
            public AuthorRole Role { get; set; } = AuthorRole.Author;
        }
    }
}