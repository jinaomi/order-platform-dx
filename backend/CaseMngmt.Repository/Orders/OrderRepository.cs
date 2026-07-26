using CaseMngmt.Models;
using CaseMngmt.Models.Database;
using CaseMngmt.Models.Orders;
using Microsoft.EntityFrameworkCore;

namespace CaseMngmt.Repository.Orders
{
    public class OrderRepository : IOrderRepository
    {
        private ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> AddAsync(Order order)
        {
            try
            {
                await _context.Order.AddAsync(order);
                var result = await _context.SaveChangesAsync();
                return result;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<int> DeleteAsync(Guid id, Guid companyId, Guid currentUserId)
        {
            try
            {
                var order = await _context.Order
                    .Include(x => x.OrderItems)
                    .FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId && !x.Deleted);

                if (order == null)
                {
                    return 0;
                }

                order.Deleted = true;
                order.UpdatedBy = currentUserId;
                order.UpdatedDate = DateTime.UtcNow;
                foreach (var item in order.OrderItems)
                {
                    item.Deleted = true;
                }

                await _context.SaveChangesAsync();
                return 1;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<PagedResult<Order>?> GetAllAsync(Guid companyId, string? status, Guid? customerId, DateTime? orderDateFrom, DateTime? orderDateTo, int pageSize, int pageNumber)
        {
            try
            {
                var queryableOrder = _context.Order
                    .Include(x => x.Customer)
                    .Include(x => x.OrderItems.Where(i => !i.Deleted))
                    .Where(x => !x.Deleted && x.CompanyId == companyId);

                if (!string.IsNullOrEmpty(status))
                {
                    queryableOrder = queryableOrder.Where(x => x.Status == status);
                }

                if (customerId.HasValue)
                {
                    queryableOrder = queryableOrder.Where(x => x.CustomerId == customerId.Value);
                }

                if (orderDateFrom.HasValue)
                {
                    queryableOrder = queryableOrder.Where(x => x.OrderDate >= orderDateFrom.Value.Date);
                }

                if (orderDateTo.HasValue)
                {
                    queryableOrder = queryableOrder.Where(x => x.OrderDate < orderDateTo.Value.Date.AddDays(1));
                }

                queryableOrder = queryableOrder.OrderByDescending(x => x.OrderDate);
                var result = await PagedResult<Order>.CreateAsync(queryableOrder.AsNoTracking(), pageNumber, pageSize);
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Order?> GetByIdAsync(Guid id, Guid companyId)
        {
            try
            {
                return await _context.Order
                    .Include(x => x.Customer)
                    .Include(x => x.OrderItems.Where(i => !i.Deleted))
                    .FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId && !x.Deleted);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<int> GetOrderCountAsync(Guid companyId, int year)
        {
            try
            {
                return await _context.Order.CountAsync(x => x.CompanyId == companyId && x.OrderDate.Year == year);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<List<Order>> GetAllForDashboardAsync(Guid companyId)
        {
            try
            {
                return await _context.Order
                    .Include(x => x.Customer)
                    .Include(x => x.OrderItems.Where(i => !i.Deleted))
                    .Where(x => !x.Deleted && x.CompanyId == companyId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<Order>();
            }
        }

        public async Task<Dictionary<Guid, decimal>> GetCommittedQuantitiesAsync(Guid companyId, Guid excludeOrderId)
        {
            try
            {
                var committedStatuses = new[] { "Confirmed", "RiskFlagged" };

                var grouped = await _context.OrderItem
                    .Include(x => x.Order)
                    .Where(x => !x.Deleted
                        && x.ProductId.HasValue
                        && x.Order != null
                        && !x.Order.Deleted
                        && x.Order.CompanyId == companyId
                        && x.Order.Id != excludeOrderId
                        && committedStatuses.Contains(x.Order.Status))
                    .GroupBy(x => x.ProductId!.Value)
                    .Select(g => new { ProductId = g.Key, Total = g.Sum(x => x.Quantity) })
                    .ToListAsync();

                return grouped.ToDictionary(x => x.ProductId, x => x.Total);
            }
            catch (Exception)
            {
                return new Dictionary<Guid, decimal>();
            }
        }

        public async Task<int> UpdateAsync(Order order, List<OrderItem> newItems)
        {
            try
            {
                // order is already tracked (loaded via GetByIdAsync in the same context),
                // so the change tracker picks up field modifications automatically.
                // New OrderItems are added explicitly via DbSet.Add() rather than through
                // the tracked entity's navigation collection, since appending to an
                // already-tracked parent's collection can get the new child misidentified
                // as Modified instead of Added, causing a 0-rows-affected UPDATE.
                foreach (var item in newItems)
                {
                    _context.OrderItem.Add(item);
                }

                await _context.SaveChangesAsync();
                return 1;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<int> UpdateStatusAsync(Guid orderId, Guid companyId, string status, Guid currentUserId)
        {
            try
            {
                var order = await _context.Order.FirstOrDefaultAsync(x => x.Id == orderId && x.CompanyId == companyId && !x.Deleted);
                if (order == null)
                {
                    return 0;
                }

                order.Status = status;
                order.UpdatedBy = currentUserId;
                order.UpdatedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return 1;
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
