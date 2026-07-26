using CaseMngmt.Models;
using CaseMngmt.Models.Orders;

namespace CaseMngmt.Service.Orders
{
    public interface IOrderService
    {
        Task<Guid?> CreateOrderAsync(OrderRequest request, Guid currentUserId);
        Task<PagedResult<OrderViewModel>?> GetAllOrdersAsync(Guid companyId, string? status, Guid? customerId, DateTime? orderDateFrom, DateTime? orderDateTo, int pageSize, int pageNumber);
        Task<OrderViewModel?> GetByIdAsync(Guid id, Guid companyId);
        Task<int> UpdateOrderAsync(Guid id, OrderRequest request, Guid currentUserId);
        Task<int> UpdateStatusAsync(Guid id, Guid companyId, string status, Guid currentUserId);
        Task<int> DeleteAsync(Guid id, Guid companyId, Guid currentUserId);
    }
}
