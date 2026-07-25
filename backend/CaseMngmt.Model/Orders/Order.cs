using CaseMngmt.Models.AiMatching;
using CaseMngmt.Models.Customers;
using CaseMngmt.Models.Products;
using System.ComponentModel.DataAnnotations;

namespace CaseMngmt.Models.Orders
{
    public class Order : BaseModel
    {
        [Required]
        public Guid CompanyId { get; set; }

        [Required]
        public Guid CustomerId { get; set; }

        [MaxLength(50)]
        public string OrderNumber { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        public DateTime? RequestedDeliveryDate { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Draft";

        [Required]
        [MaxLength(20)]
        public string SourceType { get; set; } = "Manual";

        [MaxLength(500)]
        public string? SourceDocumentPath { get; set; }

        public decimal SubTotalAmount { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal TotalAmount { get; set; }

        [MaxLength(3000)]
        public string? Note { get; set; }

        public Customer? Customer { get; set; }

        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public List<OrderRiskLineResult> RiskAssessments { get; set; } = new List<OrderRiskLineResult>();
    }
}
