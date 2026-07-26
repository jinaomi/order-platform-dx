namespace CaseMngmt.Models.Orders
{
    public class OrderExtractionResult
    {
        public string? CustomerNameGuess { get; set; }
        public double CustomerNameConfidence { get; set; } = 0.5;
        public Guid? CustomerIdMatch { get; set; }
        public DateTime? OrderDateGuess { get; set; }
        public DateTime? RequestedDeliveryDateGuess { get; set; }
        public List<OrderExtractionItem> Items { get; set; } = new();
    }

    public class OrderExtractionItem
    {
        public string ProductNameRaw { get; set; } = string.Empty;
        public Guid? ProductIdMatch { get; set; }
        public decimal Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public double Confidence { get; set; }
    }
}
