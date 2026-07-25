using CaseMngmt.Models;
using CaseMngmt.Models.CaseKeywords;
using CaseMngmt.Models.FileUploads;

namespace CaseMngmt.Repository.CaseKeywords
{
    public interface ICaseKeywordRepository
    {
        Task<int> AddMultiAsync(List<CaseKeyword> caseKeys);
        Task<int> AddAsync(CaseKeyword caseKey);
        Task<PagedResult<CaseKeywordViewModel>?> GetAllAsync(CaseKeywordSearchRequest searchRequest);
        Task<PagedResult<CaseKeywordBaseValue>?> GetDocumentsAsync(DocumentSearchRequest searchRequest);
        Task<IEnumerable<CaseKeywordBaseValue>?> GetByIdAsync(Guid caseId, bool isAdmin);
        Task<CaseKeyword?> GetByCaseIdAndKeywordIdAsync(Guid caseId, Guid keywordId);
        Task<IEnumerable<FileResponse>> GetFileKeywordsByCaseIdAsync(Guid caseId);
        Task<int> DeleteAsync(Guid id);
        Task<int> DeleteByCaseIdAsync(Guid caseId);
        Task<int> UpdateAsync(CaseKeyword caseKey);
        Task<int> UpdateMultiAsync(Guid caseId, List<CaseKeyword> caseKeys);
        Task<CaseKeyword?> GetByCustomerIdAsync(Guid customerId);
    }
}
