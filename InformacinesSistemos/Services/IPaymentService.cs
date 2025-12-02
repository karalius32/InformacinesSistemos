namespace InformacinesSistemos.Services
{
    public interface IPaymentService
    {
        Task HandleInvoicePaidAsync(int invoiceId);
        Task HandleInvoiceFailedAsync(int invoiceId);
    }
}
