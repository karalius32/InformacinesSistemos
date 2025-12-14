using System.ComponentModel.DataAnnotations;

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
    }
}