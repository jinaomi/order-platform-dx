using CaseMngmt.Models.KeywordRoles;

namespace CaseMngmt.Service.KeywordRoles
{
    public interface IKeywordRoleService
    {
        Task<int> AddMultiAsync(List<KeywordRole> keywordRoles);
        Task<List<KeywordRole>?> GetByRoleIdAsync(Guid roleId);
        Task<int> UpdateMultiAsync(Guid roleId, List<KeywordRole> keywordRoles);
    }
}
