using CaseMngmt.Models.Database;
using CaseMngmt.Models.Customers;
using Microsoft.EntityFrameworkCore;
using CaseMngmt.Models;

namespace CaseMngmt.Repository.Customers
{
    public class CustomerRepository : ICustomerRepository
    {
        private ApplicationDbContext _context;

        public CustomerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> AddAsync(Customer customer)
        {
            try
            {
                await _context.Customer.AddAsync(customer);
                var result = _context.SaveChanges();

                return result;
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
                Customer? customer = await _context.Customer.FindAsync(id);
                if (customer != null)
                {
                    customer.UpdatedBy = currentUserId;
                    customer.UpdatedDate = DateTime.UtcNow;
                    customer.Deleted = true;
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

        public async Task<PagedResult<Customer>?> GetAllAsync(string? customerName, string? phoneNumber, string companyId, int pageSize, int pageNumber)
        {
            try
            {
                var queryableCustomer = (from tempCustomer in _context.Customer select tempCustomer).Where(x => !x.Deleted);

                if (!string.IsNullOrEmpty(companyId))
                {
                    queryableCustomer = queryableCustomer.Where(m => m.CompanyId == Guid.Parse(companyId));
                }

                if (!string.IsNullOrEmpty(customerName))
                {
                    queryableCustomer = queryableCustomer.Where(m => m.Name.Contains(customerName.Trim()));
                }
                if (!string.IsNullOrEmpty(phoneNumber))
                {
                    queryableCustomer = queryableCustomer.Where(m => m.PhoneNumber.Contains(phoneNumber.Trim()));
                }

                queryableCustomer = queryableCustomer.OrderBy(m => m.Name);
                var query = queryableCustomer.AsQueryable();
                var result = await PagedResult<Customer>.CreateAsync(query.AsNoTracking(), pageNumber, pageSize);

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<Customer>> GetAllAsync()
        {
            try
            {
                var queryableCustomer = (from tempCustomer in _context.Customer select tempCustomer).Where(x => !x.Deleted);
                queryableCustomer = queryableCustomer.OrderBy(m => m.Name);
                var result = await queryableCustomer.ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                return new List<Customer>();
            }
        }

        public async Task<List<Customer>> GetAllAsync(Guid companyId)
        {
            try
            {
                var result = await _context.Customer
                    .Where(x => !x.Deleted && x.CompanyId == companyId)
                    .OrderBy(m => m.Name)
                    .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                return new List<Customer>();
            }
        }

        public async Task<Customer?> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.Customer.FindAsync(id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<Customer?> GetCustomerByNameAndPhoneAsync(string customerName, string phoneNumber)
        {
            try
            {
                var result = await (from tempCustomer in _context.Customer select tempCustomer)
                    .Where(x => !x.Deleted && x.Name == customerName && x.PhoneNumber == phoneNumber).FirstOrDefaultAsync();
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<int> UpdateAsync(Customer customer)
        {
            try
            {
                if (customer != null)
                {
                    _context.Customer.Update(customer);
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
