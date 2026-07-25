using CaseMngmt.Models.Dashboard;
using CaseMngmt.Repository.Invoices;
using CaseMngmt.Repository.Orders;

namespace CaseMngmt.Service.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IInvoiceRepository _invoiceRepository;

        private static readonly string[] RevenueStatuses = { "Confirmed", "RiskFlagged", "Invoiced" };

        public DashboardService(IOrderRepository orderRepository, IInvoiceRepository invoiceRepository)
        {
            _orderRepository = orderRepository;
            _invoiceRepository = invoiceRepository;
        }

        public async Task<DashboardSummaryViewModel> GetSummaryAsync(Guid companyId)
        {
            var orders = await _orderRepository.GetAllForDashboardAsync(companyId);
            var invoices = await _invoiceRepository.GetAllForDashboardAsync(companyId);

            var revenueOrders = orders.Where(o => RevenueStatuses.Contains(o.Status)).ToList();

            var result = new DashboardSummaryViewModel
            {
                TotalOrders = orders.Count,
                TotalOrderAmount = revenueOrders.Sum(o => o.TotalAmount),
                TotalInvoicedAmount = invoices.Sum(i => i.TotalAmount),
                RiskFlaggedCount = orders.Count(o => o.Status == "RiskFlagged"),
                OrderFunnel = orders
                    .GroupBy(o => o.Status)
                    .Select(g => new StatusCountItem { Status = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList(),
                MonthlySales = revenueOrders
                    .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                    .Select(g => new MonthlySalesItem
                    {
                        Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                        OrderCount = g.Count(),
                        TotalAmount = g.Sum(o => o.TotalAmount)
                    })
                    .OrderBy(x => x.Month)
                    .ToList(),
                TopCustomers = revenueOrders
                    .Where(o => o.Customer != null)
                    .GroupBy(o => o.Customer!.Name)
                    .Select(g => new CustomerSalesItem
                    {
                        CustomerName = g.Key,
                        OrderCount = g.Count(),
                        TotalAmount = g.Sum(o => o.TotalAmount)
                    })
                    .OrderByDescending(x => x.TotalAmount)
                    .Take(5)
                    .ToList(),
                TopProducts = revenueOrders
                    .SelectMany(o => o.OrderItems)
                    .GroupBy(i => i.ProductNameRaw)
                    .Select(g => new ProductSalesItem
                    {
                        ProductName = g.Key,
                        TotalQuantity = g.Sum(i => i.Quantity),
                        TotalAmount = g.Sum(i => i.LineAmount)
                    })
                    .OrderByDescending(x => x.TotalAmount)
                    .Take(5)
                    .ToList()
            };

            return result;
        }
    }
}
