using AutoMapper;
using CaseMngmt.Models.Customers;
using CaseMngmt.Repository.Customers;

namespace CaseMngmt.Service.Customers
{
    public class CustomerService : ICustomerService
    {
        private ICustomerRepository _repository;
        private readonly IMapper _mapper;
        public CustomerService(ICustomerRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Guid?> AddCustomerAsync(CustomerRequest customer)
        {
            try
            {
                var entity = _mapper.Map<Customer>(customer);
                entity.CompanyId = customer.CompanyId;
                entity.CreatedBy = customer.CreatedBy.Value;
                entity.UpdatedBy = customer.UpdatedBy.Value;
                var result = await _repository.AddAsync(entity);

                if (result > 0)
                {
                    return entity.Id;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<int> DeleteAsync(Guid id, Guid currenUserId)
        {
            try
            {
                return await _repository.DeleteAsync(id, currenUserId);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<Models.PagedResult<CustomerViewModel>?> GetAllCustomersAsync(string? customerName, string? phoneNumber, string companyId, int pageSize, int pageNumber)
        {
            try
            {
                var customersFromRepository = await _repository.GetAllAsync(customerName, phoneNumber, companyId, pageSize, pageNumber);

                var result = _mapper.Map<Models.PagedResult<CustomerViewModel>>(customersFromRepository);

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<CustomerViewModel?> GetByIdAsync(Guid id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);
                var result = _mapper.Map<CustomerViewModel>(entity);
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<int> UpdateCustomerAsync(Guid Id, CustomerRequest customer)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(Id);
                if (entity == null)
                {
                    return 0;
                }

                entity.Name = customer.Name;
                entity.RoomNumber = customer.RoomNumber;
                entity.City = customer.City;
                entity.PhoneNumber = customer.PhoneNumber;
                entity.BuildingName = customer.BuildingName;
                entity.City = customer.City;
                entity.Note = customer.Note;
                entity.PostCode1 = customer.PostCode1;
                entity.PostCode2 = customer.PostCode2;
                entity.StateProvince = customer.StateProvince;
                entity.CompanyId = customer.CompanyId;
                entity.Street = customer.Street;
                entity.UpdatedBy = customer.UpdatedBy.Value;
                entity.UpdatedDate = DateTime.UtcNow;
                await _repository.UpdateAsync(entity);
                return 1;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<Customer?> GetCustomerByNameAndPhoneAsync(string customerName, string phoneNumber)
        {
            try
            {
                var result = await _repository.GetCustomerByNameAndPhoneAsync(customerName, phoneNumber);
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
