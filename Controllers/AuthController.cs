using GymManagement.Api.Common;
using GymManagement.Api.DTOs;
using GymManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAccountService _service;
        private readonly JwtService _jwtService;

        public AuthController(IAccountService service, JwtService jwtService)
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

        [HttpPost("change-password")]
        [Authorize(Roles = "Admin,Member")]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest req)
        {
            // AccountId luôn lấy từ JWT claims — KHÔNG tin accountId do client gửi.
            var accountId = User.GetAccountId();
            if (accountId == null)
                return Unauthorized(new { message = "Không xác định được tài khoản từ token." });

            var (success, message) = _service.ChangePassword(accountId.Value, req);
            if (!success)
                return BadRequest(new { message });

            return Ok(new { message });
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
