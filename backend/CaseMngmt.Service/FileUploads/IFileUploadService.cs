using CaseMngmt.Models;
using CaseMngmt.Models.CaseKeywords;
using CaseMngmt.Models.FileUploads;

namespace CaseMngmt.Service.FileUploads
{
    public interface IFileUploadService
    {
        Task<FileUploadResponse?> UploadFileAsync(CaseKeywordFileUpload fileUpload, FileUploadSetting fileSetting, AWSSetting? awsSetting);
        Task<int> DeleteFileAsync(string filename, Guid caseId, FileUploadSetting fileSetting, AWSSetting? awsSetting);
        Task<List<string?>> GetAllFileByCaseIdAsync(Guid caseId, FileUploadSetting fileSetting, AWSSetting? awsSetting);
        Task<string?> GetUploadedFolderPath(Guid caseId, FileUploadSetting fileSetting, AWSSetting? awsSetting);
        Task<string?> GetFilePath(string filename, Guid caseId, FileUploadSetting fileSetting, AWSSetting? awsSetting);
        Task<byte[]?> DownloadFileS3Async(string fileName, AWSSetting awsSetting);
    }
}
