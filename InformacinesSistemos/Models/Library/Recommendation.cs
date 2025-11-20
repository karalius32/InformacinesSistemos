using System;
using System.Collections.Generic;

namespace InformacinesSistemos.Models.Library;

public partial class Recommendation
{
    public int Id { get; set; }

    public DateTime? RecommendationDate { get; set; }

    public string? RecommendationText { get; set; }

    public int LoanId { get; set; }

    public int BookId { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual Loan Loan { get; set; } = null!;
}
