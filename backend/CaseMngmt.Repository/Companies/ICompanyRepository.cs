using CaseMngmt.Models;
using CaseMngmt.Models.Companies;

namespace CaseMngmt.Repository.Companies
{
    public interface ICompanyRepository
    {
        Task<int> AddAsync(Company model);
        Task<PagedResult<Company>?> GetAllAsync(string? name, string? phoneNumber, int pageSize, int pageNumber);
        Task<Company?> GetByIdAsync(Guid id);
        Task<int> DeleteAsync(Guid id);
        Task<int> UpdateAsync(Company model);
    }
}
