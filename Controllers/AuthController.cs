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

        public AuthController(AccountService service) => _service = service;

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest req)
        {
            var (success, message, row) = _service.Login(req.Username, req.Password);

            if (!success)
                return Unauthorized(new { message });

            var result = new
            {
                accountId = row!.Table.Columns.Contains("AccountId") ? row["AccountId"] : row["MaTK"],
                fullName = (row.Table.Columns.Contains("FullName") ? row["FullName"] : row["HoTen"])?.ToString(),
                role = (row.Table.Columns.Contains("Role") ? row["Role"] : row["VaiTro"])?.ToString(),
                branchId = GetNullableValue(row, "BranchId", "MaCN")
            };

            return Ok(result);
        }

        private static object? GetNullableValue(System.Data.DataRow row, string col1, string col2)
        {
            string col = row.Table.Columns.Contains(col1) ? col1 : (row.Table.Columns.Contains(col2) ? col2 : string.Empty);
            if (string.IsNullOrEmpty(col) || row[col] == DBNull.Value) return null;
            return row[col];
        }
    }
}
