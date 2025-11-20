using System;
using System.Collections.Generic;

namespace InformacinesSistemos.Models.Library;

public partial class Author
{
    public int Id { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public DateOnly? BirthDate { get; set; }

    public string? Description { get; set; }

    public string? Country { get; set; }

    public DateOnly? DeathDate { get; set; }

    public virtual ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
}
