using CaseMngmt.Models.Customers;
using CaseMngmt.Service.CaseKeywords;
using CaseMngmt.Service.Customers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CaseMngmt.Server.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ILogger<CustomerController> _logger;
        private readonly ICustomerService _service;
        private readonly ICaseKeywordService _caseKeywordService;

        public CustomerController(ILogger<CustomerController> logger, ICustomerService service, ICaseKeywordService caseKeywordService)
        {
            _logger = logger;
            _service = service;
            _caseKeywordService = caseKeywordService;
        }

        [HttpGet, Route("getAll")]
        public async Task<IActionResult> GetAll(string? customerName = null, string? phoneNumber = null, int? pageSize = 25, int? pageNumber = 1)
        {
            try
            {
                var currentCompanyId = User?.FindFirst("CompanyId")?.Value;
                if (string.IsNullOrEmpty(currentCompanyId))
                {
                    return BadRequest();
                }

                var result = await _service.GetAllCustomersAsync(customerName, phoneNumber, currentCompanyId, pageSize ?? 25, pageNumber ?? 1);
                return result != null && result.Items.Any() ? Ok(result) : NotFound();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(CustomerController), true, e);
                return BadRequest();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(ModelState);
            }

            try
            {
                CustomerViewModel? result = await _service.GetByIdAsync(id);

                if (result == null)
                {
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(CustomerController), true, e);
                return BadRequest();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(CustomerRequest customer)
        {
            if (!ModelState.IsValid || customer == null)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var currentCompanyId = User?.FindFirst("CompanyId")?.Value;
                if (string.IsNullOrEmpty(currentCompanyId))
                {
                    return BadRequest();
                }

                var currentUserId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return BadRequest();
                }

                var existCustomer = await _service.GetCustomerByNameAndPhoneAsync(customer.Name, customer.PhoneNumber);
                if (existCustomer != null)
                {
                    return BadRequest("Customer is already exists.");
                }

                customer.CreatedBy = Guid.Parse(currentUserId);
                customer.UpdatedBy = Guid.Parse(currentUserId);
                customer.CompanyId = Guid.Parse(currentCompanyId);

                var result = await _service.AddCustomerAsync(customer);
                return result != null ? Ok(result) : BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(CustomerController), true, e);
                return BadRequest();
            }
        }

        [HttpPut, Route("{id}")]
        public async Task<IActionResult> Update(Guid id, CustomerRequest model)
        {
            if (!ModelState.IsValid || id == Guid.Empty)
            {
                return BadRequest();
            }

            try
            {
                var currentCompanyId = User?.FindFirst("CompanyId")?.Value;
                if (string.IsNullOrEmpty(currentCompanyId))
                {
                    return BadRequest();
                }

                var currentUserId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return BadRequest();
                }

                var existCustomer = await _service.GetCustomerByNameAndPhoneAsync(model.Name, model.PhoneNumber);
                if (existCustomer != null && existCustomer.Id != id)
                {
                    return BadRequest("Customer is already exists.");
                }

                model.CompanyId = Guid.Parse(currentCompanyId);
                model.UpdatedBy = Guid.Parse(currentUserId);

                var result = await _service.UpdateCustomerAsync(id, model);
                return result > 0 ? Ok(result) : BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(CustomerController), true, e);
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

                var caseKeyword = await _caseKeywordService.GetByCustomerIdAsync(id);
                if (caseKeyword != null)
                {
                    return BadRequest("You can't delete this customer because it is still used in one open case.");
                }

                var result = await _service.DeleteAsync(id, Guid.Parse(currentUserId));
                return result > 0 ? Ok(result) : BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(CustomerController), true, e);
                return BadRequest();
            }
        }
    }
}
