using CaseMngmt.Models.Database;
using Microsoft.EntityFrameworkCore;
using CaseMngmt.Models.CompanyTemplates;

namespace CaseMngmt.Repository.CompanyTemplates
{
    public class CompanyTemplateRepository : ICompanyTemplateRepository
    {
        private ApplicationDbContext _context;

        public CompanyTemplateRepository(ApplicationDbContext context)
        {
            _context = context;
        }
       
        public async Task<int> AddAsync(CompanyTemplate request)
        {
            try
            {
                await _context.CompanyTemplate.AddAsync(request);
                var result = _context.SaveChanges();

                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<int> AddMultiAsync(List<CompanyTemplate> request)
        {
            try
            {
                await _context.CompanyTemplate.AddRangeAsync(request);
                var result = _context.SaveChanges();

                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<List<CompanyTemplate>> GetTemplateByCompanyIdAsync(Guid companyId)
        {
            try
            {
                List<CompanyTemplate> companyTemplate = await _context.CompanyTemplate.Where(x => x.CompanyId.Equals(companyId)).ToListAsync();
                return companyTemplate;
            }
            catch (Exception)
            {
                return new List<CompanyTemplate>();
            }
        }
    }
}
