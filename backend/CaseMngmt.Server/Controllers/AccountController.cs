using CaseMngmt.Models.Account;
using CaseMngmt.Models.ApplicationRoles;
using CaseMngmt.Models.ApplicationUsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CaseMngmt.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IConfiguration _configuration;
        public AccountController(ILogger<AccountController> logger, UserManager<ApplicationUser> userManager,
                              SignInManager<ApplicationUser> signInManager, IConfiguration configuration, RoleManager<ApplicationRole> roleManager)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _roleManager = roleManager;
        }

        [AllowAnonymous]
        [HttpPost(Name = "Logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Login");
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim("CompanyId", user.CompanyId.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]));

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:ValidIssuer"],
                    audience: _configuration["Jwt:ValidAudience"],
                    expires: DateTime.UtcNow.AddHours(3),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                return Ok(new
                {
                    accessToken = new JwtSecurityTokenHandler().WriteToken(token),
                    roles = userRoles,
                    expiration = token.ValidTo
                });
            }
            return Unauthorized();
        }

        [HttpPost]
        [Route("register-admin")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = "User already exists!" });

            ApplicationUser user = new ApplicationUser()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username,
                CompanyId = model.CompanyId,
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = "User creation failed! Please check user details and try again." });

             if (!await _roleManager.RoleExistsAsync(UserRoles.SuperAdmin))
                await _roleManager.CreateAsync(new ApplicationRole(UserRoles.SuperAdmin));
            if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
            if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
                await _roleManager.CreateAsync(new ApplicationRole(UserRoles.Admin));

            if (await _roleManager.RoleExistsAsync(model.Role))
            {
                await _userManager.AddToRoleAsync(user, model.Role);
            }
            return Ok(new { Status = "Success", Message = "User created successfully!" });
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost]
        [Route("register-user")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterModel model)
        {
            var callerRoles = User?.FindAll(ClaimTypes.Role)?.Select(x => x.Value).ToList();
            if (callerRoles == null || (!callerRoles.Contains(UserRoles.Admin) && !callerRoles.Contains(UserRoles.SuperAdmin)))
                return Forbid();

            if (model.Role == null || model.Role == UserRoles.SuperAdmin)
                return StatusCode(StatusCodes.Status400BadRequest, new { Status = "Error", Message = "Invalid role" });

            var companyIdStr = User?.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdStr))
                return Unauthorized();
            model.CompanyId = Guid.Parse(companyIdStr);

            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = "このユーザー名は既に使用されています。" });

            ApplicationUser user = new ApplicationUser()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username,
                CompanyId = model.CompanyId
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(" / ", result.Errors.Select(e => e.Description));
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = errors });
            }

            if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
                await _roleManager.CreateAsync(new ApplicationRole(UserRoles.Admin));
            if (!await _roleManager.RoleExistsAsync(UserRoles.User))
                await _roleManager.CreateAsync(new ApplicationRole(UserRoles.User));

            if (await _roleManager.RoleExistsAsync(model.Role))
                await _userManager.AddToRoleAsync(user, model.Role);

            return Ok(new { Status = "Success", Message = "User created successfully!" });
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var callerRoles = User?.FindAll(ClaimTypes.Role)?.Select(x => x.Value).ToList();
            if (callerRoles == null || (!callerRoles.Contains(UserRoles.Admin) && !callerRoles.Contains(UserRoles.SuperAdmin)))
                return Forbid();

            var companyIdStr = User?.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdStr)) return BadRequest();
            var companyId = Guid.Parse(companyIdStr);

            var users = _userManager.Users.Where(u => u.CompanyId == companyId).ToList();

            var result = new List<object>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                result.Add(new
                {
                    id = u.Id,
                    userName = u.UserName,
                    email = u.Email,
                    role = roles.FirstOrDefault() ?? ""
                });
            }
            return Ok(result);
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var callerRoles = User?.FindAll(ClaimTypes.Role)?.Select(x => x.Value).ToList();
            if (callerRoles == null || (!callerRoles.Contains(UserRoles.Admin) && !callerRoles.Contains(UserRoles.SuperAdmin)))
                return Forbid();

            var companyIdStr = User?.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdStr)) return BadRequest();
            var companyId = Guid.Parse(companyIdStr);

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null || user.CompanyId != companyId) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles.Contains(UserRoles.SuperAdmin)) return Forbid();

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded ? Ok() : BadRequest(result.Errors);
        }
    }
}
