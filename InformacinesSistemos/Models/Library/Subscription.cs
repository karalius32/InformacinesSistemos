using System;
using System.Collections.Generic;
using InformacinesSistemos.Models.Enums;

namespace InformacinesSistemos.Models.Library;

public partial class Subscription
{
    public int Id { get; set; }

    public DateOnly? ValidUntil { get; set; }

    public DateOnly? PurchaseDate { get; set; }

    public double? Price { get; set; }

    public string? Status { get; set; }

    public bool? AutoRenew { get; set; }

    public int UserId { get; set; }

    public SubscriptionLevel Level { get; set; }

    public virtual Invoice? Invoice { get; set; }

    public virtual UserAccount User { get; set; } = null!;
}
