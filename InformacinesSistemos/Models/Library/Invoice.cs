using InformacinesSistemos.Data;
using InformacinesSistemos.Models.Enums;
using System;
using System.Collections.Generic;

namespace InformacinesSistemos.Models.Library;

public partial class Invoice
{
    public int Id { get; set; }

    public DateTime? CreatedDate { get; set; }
    public string Name { get; set; } = null!;

    public double Amount { get; set; }

    public string? Currency { get; set; }

    public string? CoinbaseChargeCode { get; set; }
    public string? CoinbaseHostedUrl { get; set; }

    public InvoiceStatus Status { get; set; }

    public int? SubscriptionId { get; set; }

    public int? LoanId { get; set; }

    public int UserId { get; set; }

    public virtual UserAccount User { get; set; } = null!;

    public virtual Loan? Loan { get; set; }

    public virtual Payment? Payment { get; set; }

    public virtual Subscription? Subscription { get; set; }
}
