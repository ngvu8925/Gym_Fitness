using GymManagement.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Api.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    [Authorize(Roles = "Admin")]
    public class MemberAccountsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public MemberAccountsController(AppDbContext db) => _db = db;

        [HttpGet("member-options")]
        public async Task<IActionResult> MemberOptions()
        {
            var accounts = await _db.Accounts.AsNoTracking()
                .Where(account => account.Role.ToLower() == "member")
                .OrderBy(account => account.FullName)
                .Select(account => new
                {
                    accountId = account.AccountId,
                    username = account.Username,
                    fullName = account.FullName,
                    phone = account.Phone,
                    email = account.Email,
                    memberId = _db.Members
                        .Where(member => member.AccountId == account.AccountId)
                        .Select(member => (int?)member.MemberId)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(accounts);
        }
    }
}
