using AutoMapper;
using CaseMngmt.Models;
using CaseMngmt.Models.Companies;
using CaseMngmt.Repository.Companies;

namespace CaseMngmt.Service.Companies
{
    public class CompanyService : ICompanyService
    {
        private ICompanyRepository _repository;
        private readonly IMapper _mapper;
        public CompanyService(ICompanyRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Guid?> AddAsync(CompanyRequest company)
        {
            try
            {
                var entity = _mapper.Map<Company>(company);
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

        public async Task<int> DeleteAsync(Guid id)
        {
            try
            {
                return await _repository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<PagedResult<CompanyViewModel>?> GetAllAsync(string? companyName, string? phoneNumber, int pageSize, int pageNumber)
        {
            try
            {
                var companysFromRepository = await _repository.GetAllAsync(companyName, phoneNumber, pageSize, pageNumber);

                var result = _mapper.Map<PagedResult<CompanyViewModel>>(companysFromRepository);

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<CompanyViewModel?> GetByIdAsync(Guid id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);
                var result = _mapper.Map<CompanyViewModel>(entity);
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<int> UpdateAsync(Guid id, CompanyRequest company)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity == null)
                {
                    return 0;
                }

                entity.Name = company.Name;
                entity.RoomNumber = company.RoomNumber;
                entity.City = company.City;
                entity.PhoneNumber = company.PhoneNumber;
                entity.BuildingName = company.BuildingName;
                entity.City = company.City;
                entity.Note = company.Note;
                entity.PostCode1 = company.PostCode1;
                entity.PostCode2 = company.PostCode2;
                entity.StateProvince = company.StateProvince;
                entity.UpdatedDate = DateTime.UtcNow;
                await _repository.UpdateAsync(entity);
                return 1;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
    }
}
