using CaseMngmt.Models;
using CaseMngmt.Models.CompanyTemplates;

namespace CaseMngmt.Repository.CompanyTemplates
{
    public interface ICompanyTemplateRepository
    {
        Task<int> AddMultiAsync(List<CompanyTemplate> request);
        Task<int> AddAsync(CompanyTemplate request);
        Task<List<CompanyTemplate>> GetTemplateByCompanyIdAsync(Guid companyId);
    }
}
