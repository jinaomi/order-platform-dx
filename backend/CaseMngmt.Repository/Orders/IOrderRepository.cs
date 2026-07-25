using CaseMngmt.Models;
using CaseMngmt.Models.Orders;

namespace CaseMngmt.Repository.Orders
{
    public interface IOrderRepository
    {
        Task<int> AddAsync(Order order);
        Task<PagedResult<Order>?> GetAllAsync(Guid companyId, string? status, Guid? customerId, int pageSize, int pageNumber);
        Task<Order?> GetByIdAsync(Guid id, Guid companyId);
        Task<int> UpdateAsync(Order order, List<OrderItem> newItems);
        Task<int> UpdateStatusAsync(Guid orderId, Guid companyId, string status, Guid currentUserId);
        Task<int> DeleteAsync(Guid id, Guid companyId, Guid currentUserId);
        Task<int> GetOrderCountAsync(Guid companyId, int year);
        Task<List<Order>> GetAllForDashboardAsync(Guid companyId);
    }
}
