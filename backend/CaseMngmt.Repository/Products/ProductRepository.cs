using CaseMngmt.Models;
using CaseMngmt.Models.Database;
using CaseMngmt.Models.Products;
using Microsoft.EntityFrameworkCore;

namespace CaseMngmt.Repository.Products
{
    public class ProductRepository : IProductRepository
    {
        private ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> AddAsync(Product product)
        {
            try
            {
                await _context.Product.AddAsync(product);
                var result = await _context.SaveChangesAsync();
                return result;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<int> DeleteAsync(Guid id, Guid currentUserId)
        {
            try
            {
                Product? product = await _context.Product.FindAsync(id);
                if (product != null)
                {
                    product.UpdatedBy = currentUserId;
                    product.UpdatedDate = DateTime.UtcNow;
                    product.Deleted = true;
                    await _context.SaveChangesAsync();
                    return 1;
                }
                return 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<PagedResult<Product>?> GetAllAsync(Guid companyId, string? name, int pageSize, int pageNumber)
        {
            try
            {
                var queryableProduct = _context.Product.Where(x => !x.Deleted && x.CompanyId == companyId);

                if (!string.IsNullOrEmpty(name))
                {
                    queryableProduct = queryableProduct.Where(m => m.Name.Contains(name.Trim()));
                }

                queryableProduct = queryableProduct.OrderBy(m => m.Name);
                var result = await PagedResult<Product>.CreateAsync(queryableProduct.AsNoTracking(), pageNumber, pageSize);
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<Product>> GetAllAsync(Guid companyId)
        {
            try
            {
                return await _context.Product
                    .Where(x => !x.Deleted && x.CompanyId == companyId)
                    .OrderBy(m => m.Name)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<Product>();
            }
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.Product.FindAsync(id);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<Product>> GetByIdsAsync(List<Guid> ids)
        {
            try
            {
                return await _context.Product.Where(x => !x.Deleted && ids.Contains(x.Id)).ToListAsync();
            }
            catch (Exception)
            {
                return new List<Product>();
            }
        }

        public async Task<int> UpdateAsync(Product product)
        {
            try
            {
                if (product != null)
                {
                    _context.Product.Update(product);
                    await _context.SaveChangesAsync();
                    return 1;
                }
                return 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
