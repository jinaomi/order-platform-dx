using CaseMngmt.Models;
using CaseMngmt.Models.Cases;

namespace CaseMngmt.Service.Cases
{
    public interface ICaseService
    {
        Task<int> AddAsync(string caseName);
        Task<PagedResult<Case>?> GetAllAsync(int pageSize, int pageNumber);
        Task<Case?> GetByIdAsync(Guid id);
        Task<int> DeleteAsync(Guid id, Guid currentUserId);
    }
}
