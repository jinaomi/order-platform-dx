using CaseMngmt.Models;
using CaseMngmt.Models.ApplicationRoles;
using CaseMngmt.Models.ApplicationUsers;
using CaseMngmt.Models.CaseKeywords;
using CaseMngmt.Models.FileUploads;
using CaseMngmt.Service.CaseKeywords;
using CaseMngmt.Service.CompanyTemplates;
using CaseMngmt.Service.FileUploads;
using CaseMngmt.Service.Templates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;

namespace CaseMngmt.Server.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    [Route("api/[controller]")]
    public class CaseController : ControllerBase
    {
        private readonly ILogger<CaseController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ICaseKeywordService _caseKeywordService;
        private readonly ITemplateService _templateService;
        private readonly IFileUploadService _fileUploadService;
        private readonly ICompanyTemplateService _companyTemplateService;
        private readonly IConfiguration _configuration;

        public CaseController(ILogger<CaseController> logger, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager,
            ICaseKeywordService caseKeywordService, ITemplateService templateService, IFileUploadService fileUploadService,
            ICompanyTemplateService companyTemplateService, IConfiguration configuration)
        {
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
            _caseKeywordService = caseKeywordService;
            _templateService = templateService;
            _fileUploadService = fileUploadService;
            _companyTemplateService = companyTemplateService;
            _configuration = configuration;
        }

        [HttpGet("template")]
        public async Task<IActionResult> GetTemplate()
        {
            try
            {
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

                var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    var roleNames = await _userManager.GetRolesAsync(user);
                    if (!roleNames.Any())
                    {
                        return BadRequest();
                    }

                    bool isAdmin = roleNames.Contains("Admin") || roleNames.Contains("SuperAdmin");
                    CaseTemplate? result = await _templateService.GetCaseSearchModelByIdAsync(templateId.Value, isAdmin, Guid.Parse(companyId));
                    if (result == null)
                    {
                        return BadRequest();
                    }

                    return Ok(result);
                }

                return BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(ControllerBase), true, e);
                return BadRequest();
            }
        }

        [HttpPost, Route("getAll")]
        public async Task<IActionResult> GetAll(CaseKeywordSearch request)
        {
            if (!ModelState.IsValid || request == null)
            {
                return BadRequest(ModelState);
            }

            try
            {
                if (!request.IsValidDatetime())
                {
                    return BadRequest("Invalid Datetime request");
                }

                var currentUserRole = User?.FindAll(ClaimTypes.Role)?.Select(x => x.Value)?.ToList();
                if (currentUserRole == null || currentUserRole.Count < 1)
                {
                    return BadRequest("Wrong Claim");
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

                var roleNames = User?.FindAll(ClaimTypes.Role)?.Select(x => x.Value).ToList() ?? new List<string>();
                bool isAdmin = roleNames.Contains("Admin") || roleNames.Contains("SuperAdmin");

                var searchRequest = new CaseKeywordSearchRequest
                {
                    RoleIds = new List<Guid>(),
                    IsAdmin = isAdmin,
                    CompanyId = Guid.Parse(companyId),
                    TemplateId = templateId.Value,
                    PageNumber = request.PageNumber ?? 1,
                    PageSize = request.PageSize ?? 25,
                    KeywordValues = request.KeywordValues,
                    KeywordDateValues = request.KeywordDateValues
                };

                var result = await _caseKeywordService.GetAllAsync(searchRequest);
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(CaseController), true, e);
                return BadRequest();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid caseId)
        {
            if (caseId == Guid.Empty)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var roleNames = User?.FindAll(ClaimTypes.Role)?.Select(x => x.Value).ToList() ?? new List<string>();
                bool isAdmin = roleNames.Contains("Admin") || roleNames.Contains("SuperAdmin");

                CaseKeywordViewModel? result = await _caseKeywordService.GetByIdAsync(caseId, isAdmin);

                if (result == null)
                {
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(CaseController), true, e);
                return BadRequest();
            }
        }

        [HttpPost]
        [Route("Close")]
        public async Task<IActionResult> CloseCase(Guid caseId)
        {
            if (caseId == Guid.Empty)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var currentUserId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return BadRequest();
                }

                var result = await _caseKeywordService.CloseCaseByAsync(caseId, Guid.Parse(currentUserId));

                return result > 0 ? Ok(result) : BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(CaseController), true, e);
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("file/getall")]
        public async Task<IActionResult> GetAllFileByCaseId(Guid caseId)
        {
            if (caseId == Guid.Empty)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var fileKeywordsResult = await _caseKeywordService.GetFileKeywordsByCaseIdAsync(caseId);
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
                var fileSetting = new FileUploadSetting()
                {
                    AcceptTypes = _configuration["FileUploadSettings:acceptTypes"],
                    InvalidFileExtensions = _configuration["FileUploadSettings:invalidFileExtensions"],
                    UploadFolder = _configuration["FileUploadSettings:uploadFolder"],
                    ValidFileTypes = _configuration["FileUploadSettings:validFileTypes"],
                };

                var availableFileResult = await _fileUploadService.GetAllFileByCaseIdAsync(caseId, fileSetting, awsSetting);

                var result = fileKeywordsResult.ToList();
                if (availableFileResult != null && fileKeywordsResult.Count() != availableFileResult.Count())
                {
                    var availableFileNames = availableFileResult
                        .Select(p => Path.GetFileName(p))
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);
                    result = fileKeywordsResult
                        .Where(x => availableFileNames.Contains(Path.GetFileName(x.FilePath)))
                        .ToList();
                }

                return result.Any()
                    ? Ok(result)
                    : NotFound();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(CaseController), true, e);
                return BadRequest();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(CaseKeywordAddRequest request)
        {
            if (!ModelState.IsValid || request == null)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var currentUserId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return BadRequest();
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

                var userTemplate = await _templateService.GetByIdAsync(templateId.Value);
                if (userTemplate == null)
                {
                    return BadRequest();
                }

                var isInValidModel = request.KeywordValues.Any(x => !x.IsValidModel());
                if (isInValidModel)
                {
                    return BadRequest("KeywordValues is wrong format");
                }

                var userKeywordSetting = userTemplate.Keywords.Select(x => x.KeywordId).ToList();
                var requestKeywords = request.KeywordValues.Select(x => x.KeywordId).ToList();

                var isValidKeywords = requestKeywords.All(id => userKeywordSetting.Contains(id));
                if (!isValidKeywords)
                {
                    return BadRequest("KeywordValues is wrong");
                }

                request.UpdatedBy = Guid.Parse(currentUserId);
                request.CreatedBy = Guid.Parse(currentUserId);
                var result = await _caseKeywordService.AddAsync(request);

                return result != null ? Ok(result) : BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(CaseController), true, e);
                return BadRequest();
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update(CaseKeywordRequest request)
        {
            if (!ModelState.IsValid || request == null || request.CaseId == Guid.Empty)
            {
                return BadRequest();
            }

            try
            {
                var currentUserId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return BadRequest();
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

                var userTemplate = await _templateService.GetByIdAsync(templateId.Value);
                if (userTemplate == null)
                {
                    return BadRequest();
                }

                var isInValidModel = request.KeywordValues.Any(x => !x.IsValidModel());
                if (isInValidModel)
                {
                    return BadRequest("KeywordValues is wrong format");
                }

                var userKeywordSetting = userTemplate.Keywords.Select(x => x.KeywordId).ToList();
                var requestKeywords = request.KeywordValues.Select(x => x.KeywordId).ToList();

                var allOfUserKeywordsIsInRequest = userKeywordSetting.Intersect(requestKeywords).Count() == userKeywordSetting.Count();
                if (!allOfUserKeywordsIsInRequest)
                {
                    return BadRequest("KeywordValues is wrong");
                }

                request.UpdatedBy = Guid.Parse(currentUserId);
                var result = await _caseKeywordService.UpdateAsync(request);
                return result > 0 ? Ok(result) : BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(CaseController), true, e);
                return BadRequest();
            }
        }

        [HttpDelete, Route("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var currentUserId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return BadRequest();
                }

                var result = await _caseKeywordService.DeleteAsync(id, Guid.Parse(currentUserId));
                return result > 0 ? Ok(result) : BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(CaseController), true, e);
                return BadRequest();
            }
        }
    }
}
