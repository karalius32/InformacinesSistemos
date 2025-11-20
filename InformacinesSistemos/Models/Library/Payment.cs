using System;
using System.Collections.Generic;

namespace InformacinesSistemos.Models.Library;

public partial class Payment
{
    public int Id { get; set; }

    public string? PaymentNumber { get; set; }

    public DateTime? PaymentDate { get; set; }

    public double? Amount { get; set; }

    public string? Status { get; set; }

    public string? TransactionId { get; set; }

    public int InvoiceId { get; set; }

    public virtual Invoice Invoice { get; set; } = null!;
}
