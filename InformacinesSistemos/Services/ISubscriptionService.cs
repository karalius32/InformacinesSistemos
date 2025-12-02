namespace InformacinesSistemos.Services
{
    public interface ISubscriptionService
    {
        Task HandleInvoicePaidAsync(int invoiceId);
        Task HandleInvoiceFailedAsync(int invoiceId);
    }
}
