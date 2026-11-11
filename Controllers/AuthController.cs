using GymManagement.Api.DTOs;
using GymManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AccountService _service;
        private readonly JwtService _jwtService;

        public AuthController(AccountService service, JwtService jwtService)
        {
            _service = service;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest req)
        {
            var (success, message, account) = _service.Login(req.Username, req.Password);

            if (!success || account == null)
                return Unauthorized(new { message });

            var token = _jwtService.GenerateToken(account.AccountId, req.Username, account.Role, account.BranchId);

            return Ok(new
            {
                token,
                accountId = account.AccountId,
                fullName = account.FullName,
                role = account.Role,
                branchId = account.BranchId
            });
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest req)
        {
            var (success, message, accountId) = _service.Register(
                req.Username, req.Password, req.FullName, req.Phone, req.Email);

            if (!success)
                return BadRequest(new { message });

            return Ok(new
            {
                message,
                accountId,
                fullName = req.FullName,
                role = "Member"
            });
        }
    }
}
