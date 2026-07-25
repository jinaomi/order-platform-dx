using CaseMngmt.Models;
using CaseMngmt.Models.Customers;

namespace CaseMngmt.Repository.Customers
{
    public interface ICustomerRepository
    {
        Task<int> AddAsync(Customer customer);
        Task<PagedResult<Customer>?> GetAllAsync(string? customerName, string? phoneNumber, string companyId, int pageSize, int pageNumber);
        Task<List<Customer>> GetAllAsync();
        Task<List<Customer>> GetAllAsync(Guid companyId);
        Task<Customer?> GetByIdAsync(Guid id);
        Task<int> DeleteAsync(Guid id, Guid currenUserId);
        Task<int> UpdateAsync(Customer customer);
        Task<Customer?> GetCustomerByNameAndPhoneAsync(string customerName, string phoneNumber);
    }
}
