using CaseMngmt.Models;
using CaseMngmt.Models.Products;

namespace CaseMngmt.Repository.Products
{
    public interface IProductRepository
    {
        Task<int> AddAsync(Product product);
        Task<PagedResult<Product>?> GetAllAsync(Guid companyId, string? name, int pageSize, int pageNumber);
        Task<List<Product>> GetAllAsync(Guid companyId);
        Task<Product?> GetByIdAsync(Guid id);
        Task<List<Product>> GetByIdsAsync(List<Guid> ids);
        Task<int> UpdateAsync(Product product);
        Task<int> DeleteAsync(Guid id, Guid currentUserId);
    }
}
