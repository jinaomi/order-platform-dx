using Type = CaseMngmt.Models.Types.Type;

namespace CaseMngmt.Repository.Types
{
    public interface ITypeRepository
    {
        Task<int> AddAsync(Type Type);
        Task<int> AddMultiAsync(List<Type> Types);
        Task<IEnumerable<Type>?> GetAllAsync();
        Task<IEnumerable<Type>?> GetAllFileTypeAsync();
        Task<Type?> GetByIdAsync(Guid id);
        Task<Type?> GetByTypeNameAsync(string name);
        Task<int> DeleteAsync(Guid id);
        Task<int> UpdateAsync(Type Type);
        Task<int> DeleteByIdsAsync(List<Guid> typeIds);
    }
}
