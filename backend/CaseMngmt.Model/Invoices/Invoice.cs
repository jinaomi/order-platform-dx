using CaseMngmt.Models.Customers;
using CaseMngmt.Models.Orders;
using System.ComponentModel.DataAnnotations;

namespace CaseMngmt.Models.Invoices
{
    public class Invoice : BaseModel
    {
        [Required]
        public Guid OrderId { get; set; }

        [Required]
        public Guid CompanyId { get; set; }

        [Required]
        public Guid CustomerId { get; set; }

        [MaxLength(50)]
        public string InvoiceNumber { get; set; }

        [Required]
        public DateTime IssueDate { get; set; }

        public DateTime? DueDate { get; set; }

        public decimal SubTotalAmount { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal TotalAmount { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Issued";

        [MaxLength(500)]
        public string? PdfPath { get; set; }

        public Order? Order { get; set; }

        public Customer? Customer { get; set; }
    }
}
