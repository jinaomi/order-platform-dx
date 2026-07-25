using CaseMngmt.Models.Orders;
using System.ComponentModel.DataAnnotations;

namespace CaseMngmt.Models.AiMatching
{
    public class OrderRiskLineResult : BaseModel
    {
        [Required]
        public Guid OrderId { get; set; }

        [Required]
        public Guid OrderItemId { get; set; }

        [Required]
        [MaxLength(20)]
        public string RiskLevel { get; set; } = "Sufficient";

        [Required]
        [MaxLength(1000)]
        public string Reasoning { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? SuggestedAction { get; set; }

        [Required]
        public DateTime EvaluatedAt { get; set; }

        public Order? Order { get; set; }

        public OrderItem? OrderItem { get; set; }
    }
}
