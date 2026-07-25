using CaseMngmt.Models;
using CaseMngmt.Models.CaseKeywords;
using CaseMngmt.Models.FileUploads;
using CaseMngmt.Service.CaseKeywords;
using CaseMngmt.Service.CompanyTemplates;
using CaseMngmt.Service.FileUploads;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace CaseMngmt.Server.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    [Route("api/[controller]")]
    public class FileUploadController : ControllerBase
    {
        private readonly ILogger<FileUploadController> _logger;
        private readonly IFileUploadService _fileUploadService;
        private readonly ICaseKeywordService _caseKeywordService;
        private readonly ICompanyTemplateService _companyTemplateService;
        private readonly IConfiguration _configuration;

        public FileUploadController(ILogger<FileUploadController> logger, IFileUploadService fileUploadService, ICaseKeywordService caseKeywordService, ICompanyTemplateService companyTemplateService, IConfiguration configuration)
        {
            _logger = logger;
            _fileUploadService = fileUploadService;
            _caseKeywordService = caseKeywordService;
            _companyTemplateService = companyTemplateService;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("Upload")]
        public async Task<IActionResult> UploadFile([FromForm] CaseKeywordFileUpload fileUploadRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                if (fileUploadRequest.FileToUpload == null)
                {
                    return BadRequest();
                }

                var awsSetting = GetAWSSetting();
                var fileSetting = GetFileUploadSetting();

                if (!fileUploadRequest.Validate(fileSetting))
                {
                    return BadRequest("Your file is not supported");
                }

                var companyId = User?.FindFirst("CompanyId")?.Value;
                if (string.IsNullOrEmpty(companyId))
                {
                    return BadRequest();
                }
                var companyTemplate = await _companyTemplateService.GetTemplateByCompanyIdAsync(Guid.Parse(companyId));
                var templateId = companyTemplate.FirstOrDefault()?.TemplateId;
                if (templateId == null || templateId == Guid.Empty)
                {
                    return BadRequest();
                }

                var uploadResult = await _fileUploadService.UploadFileAsync(fileUploadRequest, fileSetting, awsSetting);
                if (uploadResult != null)
                {
                    var result = await _caseKeywordService.AddFileToKeywordAsync(fileUploadRequest.CaseId, fileUploadRequest.FileTypeId, uploadResult, templateId.Value);
                    return result != null ? Ok(new FileResponse
                    {
                        FileName = uploadResult.FileName,
                        FilePath = uploadResult.FilePath,
                        KeywordId = result.Value,
                        IsImage = uploadResult.IsImage,
                    }) : BadRequest();
                }

                return BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(FileUploadController), true, e);
                return BadRequest();
            }
        }

        [HttpPost]
        [Route("Download")]
        public async Task<IActionResult> DownloadFile(DownloadFileRequest request)
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(request.FileName))
            {
                return BadRequest();
            }
            try
            {
                var awsSetting = GetAWSSetting();
                var fileSetting = GetFileUploadSetting();
                var filePath = await _fileUploadService.GetFilePath(request.FileName, request.CaseId, fileSetting, awsSetting);
                if (filePath == null)
                {
                    return BadRequest();
                }

                if (awsSetting == null)
                {
                    var provider = new FileExtensionContentTypeProvider();
                    if (!provider.TryGetContentType(filePath, out var contenttype))
                    {
                        contenttype = "application/octet-stream";
                    }

                    var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
                    string file = Convert.ToBase64String(bytes);
                    return Ok(file);
                    //return File(bytes, contenttype, Path.GetFileName(filePath));
                }
                else
                {
                    var result = await _fileUploadService.DownloadFileS3Async(filePath, awsSetting);
                    string file = Convert.ToBase64String(result);
                    return Ok(file);
                    //return result != null ? File(result, "application/octet-stream", filePath) : BadRequest();
                }

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(FileUploadController), true, e);
                return BadRequest();
            }
        }

        [HttpPut]
        [Route("Delete")]
        public async Task<IActionResult> DeleteFile(DeleteFileRequest request)
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(request.FileName))
            {
                return BadRequest();
            }
            try
            {
                var awsSetting = GetAWSSetting();
                var fileSetting = GetFileUploadSetting();

                var deleteResult = await _fileUploadService.DeleteFileAsync(request.FileName, request.CaseId, fileSetting, awsSetting);
                if (deleteResult > 0)
                {
                    var result = await _caseKeywordService.DeleteFileKeywordAsync(request.CaseId, request.KeywordId);
                    return result > 0 ? Ok(result) : BadRequest();
                }

                return BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(FileUploadController), true, e);
                return BadRequest();
            }
        }

        [HttpPut]
        [Route("BulkDelete")]
        public async Task<IActionResult> BulkDeleteFiles(List<DeleteFileRequest> requests)
        {
            if (requests == null || !requests.Any())
            {
                return BadRequest();
            }
            try
            {
                var awsSetting = GetAWSSetting();
                var fileSetting = GetFileUploadSetting();

                foreach (var request in requests)
                {
                    if (string.IsNullOrEmpty(request.FileName)) continue;

                    await _fileUploadService.DeleteFileAsync(request.FileName, request.CaseId, fileSetting, awsSetting);
                    await _caseKeywordService.DeleteFileKeywordAsync(request.CaseId, request.KeywordId);
                }

                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(FileUploadController), true, e);
                return BadRequest();
            }
        }

        private AWSSetting? GetAWSSetting()
        {
            AWSSetting? awsSetting = null;
            if (!string.IsNullOrEmpty(_configuration["AWS:S3Bucket"]))
            {
                awsSetting = new AWSSetting()
                {
                    S3Bucket = _configuration["AWS:S3Bucket"],
                    ACCESS_KEY = _configuration["AWS:ACCESS_KEY"],
                    SECRET_KEY = _configuration["AWS:SECRET_KEY"],
                    UploadFolder = _configuration["AWS:UploadFolder"]
                };
            }
            return awsSetting;
        }

        private FileUploadSetting GetFileUploadSetting()
        {
            var fileSetting = new FileUploadSetting()
            {
                AcceptTypes = _configuration["FileUploadSettings:acceptTypes"],
                InvalidFileExtensions = _configuration["FileUploadSettings:invalidFileExtensions"],
                UploadFolder = _configuration["FileUploadSettings:uploadFolder"],
                ValidFileTypes = _configuration["FileUploadSettings:validFileTypes"],
            };
            return fileSetting;
        }
    }
}
