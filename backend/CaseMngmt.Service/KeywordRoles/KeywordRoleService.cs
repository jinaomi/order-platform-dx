using AutoMapper;
using CaseMngmt.Models.KeywordRoles;
using CaseMngmt.Repository.KeywordRoles;

namespace CaseMngmt.Service.KeywordRoles
{
    public class KeywordRoleService : IKeywordRoleService
    {
        private IKeywordRoleRepository _repository;
        private readonly IMapper _mapper;
        public KeywordRoleService(IKeywordRoleRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<int> AddMultiAsync(List<KeywordRole> keywordRoles)
        {
            try
            {
                var result = await _repository.AddMultiAsync(keywordRoles);
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<List<KeywordRole>?> GetByRoleIdAsync(Guid roleId)
        {
            try
            {
                var result = await _repository.GetByRoleIdAsync(roleId);
                return result;
            }
            catch (Exception ex)
            {
                return new List<KeywordRole>();
            }
        }

        public async Task<int> UpdateMultiAsync(Guid roleId, List<KeywordRole> keywordRoles)
        {
            try
            {
                var result = await _repository.UpdateMultiAsync(roleId, keywordRoles);
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
    }
}
