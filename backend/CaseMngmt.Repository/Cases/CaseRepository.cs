using CaseMngmt.Models.Database;
using CaseMngmt.Models.Cases;
using Microsoft.EntityFrameworkCore;
using CaseMngmt.Models;

namespace CaseMngmt.Repository.Cases
{
    public class CaseRepository : ICaseRepository
    {
        private ApplicationDbContext _context;

        public CaseRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> AddAsync(Case model)
        {
            try
            {
                await _context.Case.AddAsync(model);
                var result = _context.SaveChanges();

                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<Case?> GetByIdAsync(Guid id)
        {
            try
            {
                var queryableCase = from tempCase in _context.Case
                                    join caseKeyword in _context.CaseKeyword on tempCase.Id equals caseKeyword.CaseId
                                    join keyword in _context.Keyword on caseKeyword.KeywordId equals keyword.Id
                                    where tempCase.Id == id 
                                        && keyword.IsShowOnTemplate
                                    select tempCase;

                queryableCase = queryableCase.Where(x => !x.Deleted).OrderBy(m => m.Name);

                var result = await queryableCase.FirstOrDefaultAsync();
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<PagedResult<Case>?> GetAllAsync(int pageSize, int pageNumber)
        {
            try
            {
                var queryableCase = (from tempCase in _context.Case select tempCase);
                queryableCase = queryableCase.Where(x => !x.Deleted).OrderBy(m => m.Name);

                var query = queryableCase.AsQueryable();
                var result = await PagedResult<Case>.CreateAsync(query.AsNoTracking(), pageNumber, pageSize);

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<int> UpdateAsync(Case caseModel)
        {
            try
            {
                if (caseModel != null)
                {
                    _context.Case.Update(caseModel);
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

        public async Task<int> DeleteAsync(Guid id, Guid currentUserId)
        {
            try
            {
                Case? caseModel = await _context.Case.FindAsync(id);
                if (caseModel != null)
                {
                    caseModel.Deleted = true;
                    caseModel.UpdatedDate = DateTime.UtcNow;
                    caseModel.UpdatedBy = currentUserId;
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
