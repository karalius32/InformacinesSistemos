using InformacinesSistemos.Models.Enums;

namespace InformacinesSistemos.Models.Library;

public partial class BookAuthor
{
    public int Id { get; set; }

    public int? Contribution { get; set; }

    public int BookId { get; set; }

    public int AuthorId { get; set; }

    public AuthorRole Role { get; set; }

    public virtual Author Author { get; set; } = null!;

    public virtual Book Book { get; set; } = null!;
}
