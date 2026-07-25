using CaseMngmt.Models;
using CaseMngmt.Models.Invoices;

namespace CaseMngmt.Repository.Invoices
{
    public interface IInvoiceRepository
    {
        Task<int> AddAsync(Invoice invoice);
        Task<PagedResult<Invoice>?> GetAllAsync(Guid companyId, int pageSize, int pageNumber);
        Task<Invoice?> GetByIdAsync(Guid id, Guid companyId);
        Task<Invoice?> GetByOrderIdAsync(Guid orderId, Guid companyId);
        Task<int> UpdateStatusAsync(Guid id, Guid companyId, string status, Guid currentUserId);
        Task<int> GetInvoiceCountAsync(Guid companyId, int year);
        Task<List<Invoice>> GetAllForDashboardAsync(Guid companyId);
    }
}
