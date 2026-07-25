using CaseMngmt.Models;
using CaseMngmt.Models.Companies;

namespace CaseMngmt.Service.Companies
{
    public interface ICompanyService
    {
        Task<Guid?> AddAsync(CompanyRequest Company);
        Task<PagedResult<CompanyViewModel>?> GetAllAsync(string? name, string? phoneNumber, int pageSize, int pageNumber);
        Task<CompanyViewModel?> GetByIdAsync(Guid id);
        Task<int> DeleteAsync(Guid id);
        Task<int> UpdateAsync(Guid Id, CompanyRequest Company);
    }
}
