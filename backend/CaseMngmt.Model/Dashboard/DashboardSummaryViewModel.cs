namespace CaseMngmt.Models.Dashboard
{
    public class DashboardSummaryViewModel
    {
        public int TotalOrders { get; set; }
        public decimal TotalOrderAmount { get; set; }
        public decimal TotalInvoicedAmount { get; set; }
        public int RiskFlaggedCount { get; set; }
        public List<StatusCountItem> OrderFunnel { get; set; } = new();
        public List<MonthlySalesItem> MonthlySales { get; set; } = new();
        public List<CustomerSalesItem> TopCustomers { get; set; } = new();
        public List<ProductSalesItem> TopProducts { get; set; } = new();
    }

    public class StatusCountItem
    {
        public string Status { get; set; }
        public int Count { get; set; }
    }

    public class MonthlySalesItem
    {
        public string Month { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class CustomerSalesItem
    {
        public string CustomerName { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class ProductSalesItem
    {
        public string ProductName { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
