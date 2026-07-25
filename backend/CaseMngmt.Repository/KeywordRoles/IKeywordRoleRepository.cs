using CaseMngmt.Models.KeywordRoles;

namespace CaseMngmt.Repository.KeywordRoles
{
    public interface IKeywordRoleRepository
    {
        Task<int> AddMultiAsync(List<KeywordRole> keywordRoles);
        Task<List<KeywordRole>?> GetByRoleIdAsync(Guid roleId);
        Task<int> UpdateMultiAsync(Guid roleId, List<KeywordRole> keywordRoles);
    }
}
