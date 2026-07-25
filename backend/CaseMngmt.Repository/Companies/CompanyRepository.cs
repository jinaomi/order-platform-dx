using CaseMngmt.Models.Database;
using CaseMngmt.Models.Companies;
using Microsoft.EntityFrameworkCore;
using CaseMngmt.Models;

namespace CaseMngmt.Repository.Companies
{
    public class CompanyRepository : ICompanyRepository
    {
        private ApplicationDbContext _context;

        public CompanyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> AddAsync(Company Company)
        {
            try
            {
                await _context.Company.AddAsync(Company);
                var result = _context.SaveChanges();

                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<int> DeleteAsync(Guid id)
        {
            try
            {
                Company? company = await _context.Company.FindAsync(id);
                if (company != null)
                {
                    company.Deleted = true;
                    await _context.SaveChangesAsync();
                    return 1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<PagedResult<Company>?> GetAllAsync(string? companyName, string? phoneNumber, int pageSize, int pageNumber)
        {
            try
            {
                var queryableCompany = (from tempCompany in _context.Company select tempCompany).Where(x => !x.Deleted);

                if (!string.IsNullOrEmpty(companyName))
                {
                    queryableCompany = queryableCompany.Where(m => m.Name.Contains(companyName.Trim()));
                }
                if (!string.IsNullOrEmpty(phoneNumber))
                {
                    queryableCompany = queryableCompany.Where(m => m.PhoneNumber.Contains(phoneNumber.Trim()));
                }

                queryableCompany = queryableCompany.OrderBy(m => m.Name);
                var query = queryableCompany.AsQueryable();
                var result = await PagedResult<Company>.CreateAsync(query.AsNoTracking(), pageNumber, pageSize);

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<Company?> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.Company.FindAsync(id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<int> UpdateAsync(Company company)
        {
            try
            {
                if (company != null)
                {
                    _context.Company.Update(company);
                    await _context.SaveChangesAsync();
                    return 1;
                }

                return 0;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
    }
}
