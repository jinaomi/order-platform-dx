using CaseMngmt.Models.CompanyTemplates;

namespace CaseMngmt.Service.CompanyTemplates
{
    public interface ICompanyTemplateService
    {
        //Task<int> AddAsync(CompanyTemplate request);
        //Task<CompanyTemplate> GetByIdAsync(Guid companyId);
        //Task<int> UpdateAsync(CompanyTemplate request);
        //Task<int> DeleteAsync(Guid id);
        Task<List<CompanyTemplate>> GetTemplateByCompanyIdAsync(Guid companyId);
    }
}
