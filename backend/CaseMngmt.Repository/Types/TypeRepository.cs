using CaseMngmt.Models.Database;
using Microsoft.EntityFrameworkCore;
using Type = CaseMngmt.Models.Types.Type;

namespace CaseMngmt.Repository.Types
{
    public class TypeRepository : ITypeRepository
    {
        private ApplicationDbContext _context;

        public TypeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> AddAsync(Type model)
        {
            try
            {
                await _context.Type.AddAsync(model);
                var result = _context.SaveChanges();

                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<Type?> GetByIdAsync(Guid id)
        {
            try
            {
                var result = await _context.Type.FindAsync(id);
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<IEnumerable<Type>?> GetAllAsync()
        {
            try
            {
                var IQueryableType = (from tempType in _context.Type select tempType);
                IQueryableType = IQueryableType.Where(x => !x.Deleted && x.IsDefaultType && !x.IsFileType).OrderBy(m => m.Name);
                var result = await IQueryableType.ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<IEnumerable<Type>?> GetAllFileTypeAsync()
        {
            try
            {
                var IQueryableType = (from tempType in _context.Type select tempType);
                IQueryableType = IQueryableType.Where(x => !x.Deleted && !x.IsDefaultType && x.IsFileType).OrderBy(m => m.Name);
                var result = await IQueryableType.ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<int> UpdateAsync(Type typeModel)
        {
            try
            {
                if (typeModel != null)
                {
                    _context.Type.Update(typeModel);
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

        public async Task<int> DeleteAsync(Guid id)
        {
            try
            {
                Type? typeModel = await _context.Type.FindAsync(id);
                if (typeModel != null)
                {
                    typeModel.Deleted = true;
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

        public async Task<int> AddMultiAsync(List<Type> types)
        {
            try
            {
                await _context.Type.AddRangeAsync(types);
                var result = _context.SaveChanges();

                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<int> DeleteByIdsAsync(List<Guid> typeIds)
        {
            try
            {
                var data = _context.Type.Where(a => !a.Deleted && typeIds.Contains(a.Id)).ToList();
                foreach (var item in data)
                {
                    item.Deleted = true;
                    _context.Type.Update(item);
                }
                await _context.SaveChangesAsync();

                return 1;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<Type?> GetByTypeNameAsync(string name)
        {
            try
            {
                var result = await _context.Type.FirstOrDefaultAsync(x => x.Name == name);
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
