using InformacinesSistemos.Data;
using InformacinesSistemos.Models.Enums;
using InformacinesSistemos.Models.Library;
using Microsoft.EntityFrameworkCore;

namespace InformacinesSistemos.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly LibraryContext _db;
        private readonly IConfiguration _cfg;

        public PaymentService(LibraryContext db, IConfiguration cfg)
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

            if (invoice.SubscriptionId.HasValue)
                await HandleInvoicePaidSubscription(invoice);
            else if (invoice.LoanId.HasValue)
                await HandleInvoicePaidLoan(invoice);


            await _db.SaveChangesAsync();
        }

        public async Task HandleInvoicePaidSubscription(Invoice invoice)
        {
            int durationDays = 30;
            // get or create subscription record
            var sub = await _db.Subscriptions
                .FirstOrDefaultAsync(s => s.Invoice != null && s.Invoice.Id == invoice.Id);

            if (sub == null)
                throw new Exception("Subscription not found for paid invoice.");

            var start = sub.ValidUntil.HasValue && sub.ValidUntil.Value >= DateOnly.FromDateTime(DateTime.UtcNow)
                ? sub.ValidUntil.Value
                : DateOnly.FromDateTime(DateTime.UtcNow);

            sub.ValidUntil = start.AddDays(durationDays);
            sub.PurchaseDate = DateOnly.FromDateTime(DateTime.UtcNow);
            sub.Status = SubscriptionStatus.Active;
        }

        public async Task HandleInvoicePaidLoan(Invoice invoice)
        {
            // JUOZAPAI, TAU CIA
        }

        public async Task HandleInvoiceFailedAsync(int invoiceId)
        {
            var invoice = await _db.Invoices.FindAsync(invoiceId);
            if (invoice == null) return;

            invoice.Status = InvoiceStatus.Failed;

            if (invoice.SubscriptionId.HasValue)
                await HandleInvoiceFailedSubscription(invoice);
            else if (invoice.LoanId.HasValue)
                await HandleInvoiceFailedLoan(invoice);

            await _db.SaveChangesAsync();
        }

        public async Task HandleInvoiceFailedSubscription(Invoice invoice)
        {
            var sub = await _db.Subscriptions
                .FirstOrDefaultAsync(s => s.Invoice != null && s.Invoice.Id == invoice.Id);

            if (sub != null)
                sub.Status = SubscriptionStatus.PaymentFailed;
        }

        public async Task HandleInvoiceFailedLoan(Invoice invoice)
        {
            // JUOZAPAI, TAU CIA
        }
    }

}
