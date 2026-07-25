using CaseMngmt.Models.AiMatching;
using CaseMngmt.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace CaseMngmt.Repository.AiMatching
{
    public class OrderRiskRepository : IOrderRiskRepository
    {
        private ApplicationDbContext _context;

        public OrderRiskRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> SoftDeleteByOrderIdAsync(Guid orderId)
        {
            try
            {
                var existing = await _context.OrderRiskLineResult
                    .Where(x => x.OrderId == orderId && !x.Deleted)
                    .ToListAsync();

                foreach (var item in existing)
                {
                    item.Deleted = true;
                }

                return await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<int> AddRangeAsync(List<OrderRiskLineResult> results)
        {
            try
            {
                await _context.OrderRiskLineResult.AddRangeAsync(results);
                return await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<List<OrderRiskLineResult>> GetByOrderIdAsync(Guid orderId)
        {
            try
            {
                return await _context.OrderRiskLineResult
                    .Where(x => x.OrderId == orderId && !x.Deleted)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<OrderRiskLineResult>();
            }
        }
    }
}
