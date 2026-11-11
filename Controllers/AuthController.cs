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
            var (success, message, row) = _service.Login(req.Username, req.Password);

            if (!success)
                return Unauthorized(new { message });

            var accountId = Convert.ToInt32(row!.Table.Columns.Contains("AccountId") ? row["AccountId"] : row["MaTK"]);
            var fullName = (row.Table.Columns.Contains("FullName") ? row["FullName"] : row["HoTen"])?.ToString() ?? string.Empty;
            var role = (row.Table.Columns.Contains("Role") ? row["Role"] : row["VaiTro"])?.ToString() ?? "User";
            var rawBranchId = GetNullableValue(row, "BranchId", "MaCN");
            int? branchId = rawBranchId != null ? Convert.ToInt32(rawBranchId) : null;

            var token = _jwtService.GenerateToken(accountId, req.Username, role, branchId);

            var result = new
            {
                token,
                accountId,
                fullName,
                role,
                branchId
            };

            return Ok(result);
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

        private static object? GetNullableValue(System.Data.DataRow row, string col1, string col2)
        {
            string col = row.Table.Columns.Contains(col1) ? col1 : (row.Table.Columns.Contains(col2) ? col2 : string.Empty);
            if (string.IsNullOrEmpty(col) || row[col] == DBNull.Value) return null;
            return row[col];
        }
    }
}
