namespace CaseMngmt.Models.Invoices
{
    public class InvoiceViewModel
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string? OrderNumber { get; set; }
        public Guid CompanyId { get; set; }
        public Guid CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal SubTotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string? PdfPath { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
