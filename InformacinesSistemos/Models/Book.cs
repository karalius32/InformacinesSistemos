using System;
using System.Collections.Generic;

namespace InformacinesSistemos.Models;

public partial class Book
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Identifier { get; set; }

    public DateOnly? PublishDate { get; set; }

    public DateOnly? UpdatedDate { get; set; }

    public string? Description { get; set; }

    public int? PageCount { get; set; }

    public string? Publisher { get; set; }

    public string? Language { get; set; }

    public string? Format { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? CoverUrl { get; set; }

    public string? Keywords { get; set; }

    public int? LoanId { get; set; }

    public virtual ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();

    public virtual ICollection<BookCategory> BookCategories { get; set; } = new List<BookCategory>();

    public virtual Loan? Loan { get; set; }

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();
}
