using CaseMngmt.Models.Database;
using Microsoft.EntityFrameworkCore;
using CaseMngmt.Models.KeywordRoles;
using System.Data.Entity;

namespace CaseMngmt.Repository.KeywordRoles
{
    public class KeywordRoleRepository : IKeywordRoleRepository
    {
        private ApplicationDbContext _context;

        public KeywordRoleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> AddMultiAsync(List<KeywordRole> keywordRoles)
        {
            try
            {
                await _context.KeywordRole.AddRangeAsync(keywordRoles);
                var result = _context.SaveChanges();

                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<List<KeywordRole>?> GetByRoleIdAsync(Guid roleId)
        {
            try
            {
                var query = (from tempKeyword in _context.KeywordRole select tempKeyword);
                query = query.Where(x => x.RoleId == roleId);
                var result = await query.ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<int> UpdateMultiAsync(Guid roleId, List<KeywordRole> keywordRoles)
        {
            try
            {
                var data = _context.KeywordRole.Where(a => a.RoleId == roleId).ToList();
                _context.KeywordRole.RemoveRange(data);
                var result = await _context.SaveChangesAsync();

                if (keywordRoles.Any())
                {
                    await _context.KeywordRole.AddRangeAsync(keywordRoles);
                    result = _context.SaveChanges();
                }

                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
    }
}
