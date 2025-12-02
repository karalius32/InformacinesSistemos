using InformacinesSistemos.Models.Enums;

namespace InformacinesSistemos.Models.Library;

public partial class Subscription
{
    public int Id { get; set; }

    public DateOnly? ValidUntil { get; set; }

    public DateOnly? PurchaseDate { get; set; }

    public SubscriptionStatus Status { get; set; }

    public int UserId { get; set; }

    public virtual Invoice? Invoice { get; set; }

    public virtual UserAccount User { get; set; } = null!;
}
