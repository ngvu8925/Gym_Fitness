using GymManagement.Api.Common;
using GymManagement.Api.DTOs;
using GymManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GymManagement.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAccountService _service;
        private readonly JwtService _jwtService;
        private readonly LoginThrottle _loginThrottle;

        public AuthController(IAccountService service, JwtService jwtService, LoginThrottle loginThrottle)
        {
            _service = service;
            _jwtService = jwtService;
            _loginThrottle = loginThrottle;
        }

        [HttpPost("login")]
        [EnableRateLimiting("login")]
        public IActionResult Login([FromBody] LoginRequest req)
        {
            // Rate limit theo username (theo tài khoản) — IP đã bị chặn ở middleware.
            if (_loginThrottle.IsThrottled(req.Username ?? string.Empty))
                return TooManyRequests();

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var (success, message, account) = _service.Login(req.Username, req.Password, ip);

            if (!success || account == null)
            {
                // 423 Locked khi tài khoản bị khóa tạm; 401 cho các trường hợp còn lại
                // (message chung, không tiết lộ tài khoản có tồn tại hay không).
                if (message != null && message.Contains("temporarily locked"))
                    return StatusCode(423, new { message });

                return Unauthorized(new { message });
            }

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

        private IActionResult TooManyRequests()
        {
            Response.Headers.RetryAfter = "60";
            return StatusCode(429, new { message = "Too many login attempts. Please try again later." });
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
