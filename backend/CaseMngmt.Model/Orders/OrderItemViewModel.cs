namespace CaseMngmt.Models.Orders
{
    public class OrderItemViewModel
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid? ProductId { get; set; }
        public string ProductNameRaw { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineAmount { get; set; }
        public string? Note { get; set; }
    }
}
