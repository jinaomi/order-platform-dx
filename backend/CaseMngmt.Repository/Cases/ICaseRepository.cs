using CaseMngmt.Models;
using CaseMngmt.Models.Cases;

namespace CaseMngmt.Repository.Cases
{
    public interface ICaseRepository
    {
        Task<int> AddAsync(Case model);
        Task<PagedResult<Case>?> GetAllAsync(int pageSize, int pageNumber);
        Task<Case?> GetByIdAsync(Guid id);
        Task<int> DeleteAsync(Guid id, Guid currentUserId);
        Task<int> UpdateAsync(Case model);
    }
}
