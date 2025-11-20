using System;
using System.Collections.Generic;

namespace InformacinesSistemos.Models.Library;

public partial class Loan
{
    public int Id { get; set; }

    public DateOnly? LoanDate { get; set; }

    public DateOnly? ReturnDate { get; set; }

    public double? AccumulatedPenalties { get; set; }

    public int? ExtensionCount { get; set; }

    public int UserId { get; set; }

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();

    public virtual UserAccount User { get; set; } = null!;
}
