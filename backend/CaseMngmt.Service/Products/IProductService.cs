using CaseMngmt.Models;
using CaseMngmt.Models.Products;

namespace CaseMngmt.Service.Products
{
    public interface IProductService
    {
        Task<Guid?> AddProductAsync(ProductRequest product);
        Task<PagedResult<ProductViewModel>?> GetAllProductsAsync(Guid companyId, string? name, int pageSize, int pageNumber);
        Task<List<ProductViewModel>> GetAllProductsAsync(Guid companyId);
        Task<List<Product>> GetByIdsAsync(List<Guid> ids);
        Task<ProductViewModel?> GetByIdAsync(Guid id);
        Task<int> UpdateProductAsync(Guid id, ProductRequest product);
        Task<int> DeleteAsync(Guid id, Guid currentUserId);
    }
}
