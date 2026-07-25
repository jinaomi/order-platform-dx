namespace CaseMngmt.Models.AiMatching
{
    public class OrderRiskLineResultViewModel
    {
        public Guid OrderItemId { get; set; }
        public string? ProductNameRaw { get; set; }
        public string RiskLevel { get; set; } = "Sufficient";
        public string Reasoning { get; set; } = string.Empty;
        public string? SuggestedAction { get; set; }
        public DateTime EvaluatedAt { get; set; }
    }

    public class OrderRiskSummaryViewModel
    {
        public Guid OrderId { get; set; }
        public string OverallRiskLevel { get; set; } = "Sufficient";
        public List<OrderRiskLineResultViewModel> Lines { get; set; } = new();
    }
}
