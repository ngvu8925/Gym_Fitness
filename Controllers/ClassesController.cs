using GymManagement.Api.Common;
using GymManagement.Api.DTOs;
using GymManagement.Api.DTOs.Common;
using GymManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Api.Controllers
{
    [ApiController]
    [Route("api/classes")]
    public class ClassesController : ControllerBase
    {
        private readonly IGymClassService _service;
        private readonly IMemberAccountService _memberAccounts;

        public ClassesController(IGymClassService service, IMemberAccountService memberAccounts)
        {
            _service = service;
            _memberAccounts = memberAccounts;
        }

        [HttpGet]
        public IActionResult List([FromQuery] UserQueryParameters query) => Ok(_service.List(query));

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

        [HttpPost("unenroll/{enrollmentId}")]
        [Authorize(Roles = "Member")]
        public IActionResult Unenroll(int enrollmentId)
        {
            // MemberId luôn lấy từ JWT — hội viên chỉ hủy được đăng ký của chính mình.
            var accountId = User.GetAccountId();
            if (accountId == null) return Unauthorized();
            var member = _memberAccounts.GetMemberForAccount(accountId.Value);
            if (member == null)
                return Conflict(new { message = "This account is not linked to a member profile." });

            var (success, message) = _service.Unenroll(enrollmentId, member.MemberId);
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }

        [HttpPut("{classId}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Update(int classId, [FromBody] GymClassRequest req)
        {
            var (success, message) = _service.Update(classId, req);
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }

        [HttpDelete("{classId}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int classId)
        {
            _service.Delete(classId);
            return Ok(new { message = "Class deactivated" });
        }
    }
}
