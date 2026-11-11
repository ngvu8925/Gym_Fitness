using GymManagement.Api.Common;
using GymManagement.Api.DTOs;
using GymManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Api.Controllers
{
    [ApiController]
    [Route("api/classes")]
    public class ClassesController : ControllerBase
    {
        private readonly GymClassService _service;
        private readonly MemberAccountService _memberAccounts;

        public ClassesController(GymClassService service, MemberAccountService memberAccounts)
        {
            _service = service;
            _memberAccounts = memberAccounts;
        }

        [HttpGet]
        public IActionResult List() => Ok(_service.List());

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Add([FromBody] GymClassRequest req)
        {
            var (success, message, classId) = _service.Add(req);
            if (!success) return BadRequest(new { message });
            return Ok(new { message, classId });
        }

        [HttpPost("enroll")]
        [Authorize]
        public IActionResult Enroll([FromBody] ClassEnrollmentRequest req)
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

            var (success, message, enrollmentId) = _service.Enroll(req);
            if (!success) return BadRequest(new { message });
            return Ok(new { message, enrollmentId });
        }
    }
}
