using AutoMapper;
using CaseMngmt.Models.CompanyTemplates;
using CaseMngmt.Repository.CompanyTemplates;

namespace CaseMngmt.Service.CompanyTemplates
{
    public class CompanyTemplateService : ICompanyTemplateService
    {
        private ICompanyTemplateRepository _repository;
        private readonly IMapper _mapper;
        public CompanyTemplateService(ICompanyTemplateRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<CompanyTemplate>> GetTemplateByCompanyIdAsync(Guid companyId)
        {
            try
            {
                List<CompanyTemplate> result = await _repository.GetTemplateByCompanyIdAsync(companyId);
                return result;
            }
            catch (Exception ex)
            {
                return new List<CompanyTemplate>();
            }
        }
    }
}
