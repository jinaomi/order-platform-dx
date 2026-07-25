using AutoMapper;
using CaseMngmt.Models.Types;
using CaseMngmt.Repository.Types;
using Type = CaseMngmt.Models.Types.Type;

namespace CaseMngmt.Service.Types
{
    public class TypeService : ITypeService
    {
        private ITypeRepository _repository;
        private readonly IMapper _mapper;
        public TypeService(ITypeRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<int> AddAsync(TypeRequest request)
        {
            try
            {
                var entity = _mapper.Map<Type>(request);
                return await _repository.AddAsync(entity);
            }
            catch (Exception ex)
            {
                return 0;
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

        public async Task<IEnumerable<TypeViewModel>?> GetAllAsync(bool isFileType)
        {
            try
            {
                var typesFromRepository =  isFileType 
                    ? await _repository.GetAllFileTypeAsync()
                    : await _repository.GetAllAsync();

                var result = _mapper.Map<List<TypeViewModel>>(typesFromRepository);

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<TypeViewModel?> GetByIdAsync(Guid id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);
                var result = _mapper.Map<TypeViewModel>(entity);
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<int> UpdateAsync(Guid Id, TypeRequest request)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(Id);
                if (entity == null)
                {
                    return 0;
                }

                entity.Name = request.Name;
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
