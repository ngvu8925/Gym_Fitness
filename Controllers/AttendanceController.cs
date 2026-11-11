using GymManagement.Api.Common;
using GymManagement.Api.DTOs;
using GymManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Api.Controllers
{
    [ApiController]
    [Route("api/attendance")]
    public class AttendanceController : ControllerBase
    {
        private readonly AttendanceService _service;
        private readonly MemberAccountService _memberAccounts;

        public AttendanceController(AttendanceService service, MemberAccountService memberAccounts)
        {
            _service = service;
            _memberAccounts = memberAccounts;
        }

        [HttpPost("checkin")]
        [Authorize]
        public IActionResult CheckIn([FromBody] CheckInRequest req)
        {
            if (User.IsInRole("Member"))
            {
                var accountId = User.GetAccountId();
                if (accountId == null) return Unauthorized();
                var member = _memberAccounts.GetMemberForAccount(accountId.Value);
                if (member == null)
                    return Conflict(new { message = "This account is not linked to a member profile." });
                req.MemberId = member.MemberId;
            }
            else if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var (success, message) = _service.CheckIn(req.MemberId);
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }

        [HttpPost("checkout/{attendanceId}")]
        [Authorize(Roles = "Admin")]
        public IActionResult CheckOut(int attendanceId)
        {
            _service.CheckOut(attendanceId);
            return Ok(new { message = "Check-out successful" });
        }
    }
}
