using AutoMapper;
using CaseMngmt.Models;
using CaseMngmt.Models.Cases;
using CaseMngmt.Repository.Cases;

namespace CaseMngmt.Service.Cases
{
    public class CaseService : ICaseService
    {
        private ICaseRepository _repository;
        private readonly IMapper _mapper;
        public CaseService(ICaseRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<int> AddAsync(string caseName)
        {
            try
            {
                Case entity = new Case()
                {
                    Name = caseName
                };
                return await _repository.AddAsync(entity);
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
                return await _repository.DeleteAsync(id, currentUserId);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<PagedResult<Case>?> GetAllAsync(int pageSize, int pageNumber)
        {
            try
            {
                var result = await _repository.GetAllAsync(pageSize, pageNumber);
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<Case?> GetByIdAsync(Guid id)
        {
            try
            {
                var result = await _repository.GetByIdAsync(id);
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
