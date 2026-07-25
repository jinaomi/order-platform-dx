namespace CaseMngmt.Models.Invoices
{
    public class InvoiceCreateResult
    {
        // > 0 success, 0 = order not found, -1 = business rule violation
        public int StatusCode { get; set; }
        public Guid? InvoiceId { get; set; }
        public string? Message { get; set; }
    }
}
