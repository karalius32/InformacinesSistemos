using InformacinesSistemos.Data;
using InformacinesSistemos.Models.Enums;
using InformacinesSistemos.Models.Library;
using Microsoft.EntityFrameworkCore;

namespace InformacinesSistemos.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly LibraryContext _db;
        private readonly IConfiguration _cfg;

        public SubscriptionService(LibraryContext db, IConfiguration cfg)
        {
            _db = db;
            _cfg = cfg;
        }

        public async Task HandleInvoicePaidAsync(int invoiceId)
        {
            var invoice = await _db.Invoices.FindAsync(invoiceId);
            if (invoice == null) return;

            // avoid double-processing
            if (invoice.Status == InvoiceStatus.Paid) return;

            invoice.Status = InvoiceStatus.Paid;

            var durationDays = int.Parse(_cfg["Subscription:DurationDays"] ?? "30");
            var price = invoice.Amount;

            // get or create subscription record
            var sub = await _db.Subscriptions
                .FirstOrDefaultAsync(s => s.Invoice != null && s.Invoice.Id == invoiceId);

            if (sub == null)
            {
                sub = new Subscription
                {
                    UserId = invoice.UserId,
                    PurchaseDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    Price = price,
                    Status = "Active",
                    ValidUntil = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(durationDays)),
                    Invoice = invoice
                };
                _db.Subscriptions.Add(sub);
            }
            else
            {
                // extend if already exists
                var start = sub.ValidUntil.HasValue && sub.ValidUntil.Value >= DateOnly.FromDateTime(DateTime.UtcNow)
                    ? sub.ValidUntil.Value
                    : DateOnly.FromDateTime(DateTime.UtcNow);

                sub.ValidUntil = start.AddDays(durationDays);
                sub.Status = "Active";
            }

            await _db.SaveChangesAsync();
        }

        public async Task HandleInvoiceFailedAsync(int invoiceId)
        {
            var invoice = await _db.Invoices.FindAsync(invoiceId);
            if (invoice == null) return;

            invoice.Status = InvoiceStatus.Failed;

            var sub = await _db.Subscriptions
                .FirstOrDefaultAsync(s => s.Invoice != null && s.Invoice.Id == invoiceId);

            if (sub != null)
                sub.Status = "Failed";

            await _db.SaveChangesAsync();
        }
    }

}
