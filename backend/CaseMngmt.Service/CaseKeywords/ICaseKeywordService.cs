using CaseMngmt.Models;
using CaseMngmt.Models.CaseKeywords;
using CaseMngmt.Models.FileUploads;

namespace CaseMngmt.Service.CaseKeywords
{
    public interface ICaseKeywordService
    {
        Task<Models.Cases.CaseResponse?> AddAsync(CaseKeywordAddRequest request);
        Task<PagedResult<CaseKeywordViewModel>?> GetAllAsync(CaseKeywordSearchRequest searchRequest);
        Task<PagedResult<CaseKeywordBaseValue>?> GetDocumentsAsync(DocumentSearchRequest searchRequest);
        Task<CaseKeywordViewModel?> GetByIdAsync(Guid caseId, bool isAdmin);
        Task<int> CloseCaseByAsync(Guid caseId, Guid currentUserId);
        Task<int> DeleteAsync(Guid caseId, Guid currentUserId);
        Task<int> UpdateAsync(CaseKeywordRequest request);
        Task<Guid?> AddFileToKeywordAsync(Guid caseId, Guid fileTypeId, FileUploadResponse fileResponse, Guid templateId);
        Task<IEnumerable<FileResponse>> GetFileKeywordsByCaseIdAsync(Guid caseId);
        Task<int> DeleteFileKeywordAsync(Guid caseId, Guid keywordId);
        Task<CaseKeyword?> GetByCustomerIdAsync(Guid customerId);
    }
}
