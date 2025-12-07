using System;
using System.Collections.Generic;

namespace InformacinesSistemos.Models;

public partial class Rating
{
    public int Id { get; set; }

    public int? RatingValue { get; set; }

    public DateTime? RatingDate { get; set; }

    public string? Comment { get; set; }

    public int UserId { get; set; }

    public int BookId { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual UserAccount User { get; set; } = null!;
}
