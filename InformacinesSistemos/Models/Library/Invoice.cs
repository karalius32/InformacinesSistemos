using System;
using System.Collections.Generic;

namespace InformacinesSistemos.Models.Library;

public partial class Invoice
{
    public int Id { get; set; }

    public DateTime? CreatedDate { get; set; }

    public double? Amount { get; set; }

    public string? Currency { get; set; }

    public string? Address { get; set; }

    public string? Status { get; set; }

    public string? Network { get; set; }

    public int? SubscriptionId { get; set; }

    public int? LoanId { get; set; }

    public virtual Loan? Loan { get; set; }

    public virtual Payment? Payment { get; set; }

    public virtual Subscription? Subscription { get; set; }
}
