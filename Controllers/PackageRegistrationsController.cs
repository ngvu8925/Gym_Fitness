using GymManagement.Api.Common;
using GymManagement.Api.DTOs;
using GymManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Api.Controllers
{
    [ApiController]
    [Route("api/package-registrations")]
    public class PackageRegistrationsController : ControllerBase
    {
        private readonly IPackageService _service;
        private readonly IMemberAccountService _memberAccounts;

        public PackageRegistrationsController(IPackageService service, IMemberAccountService memberAccounts)
        {
            _service = service;
            _memberAccounts = memberAccounts;
        }

        [HttpPost]
        [Authorize]
        public IActionResult Register([FromBody] PackageRegistrationRequest req)
        {
            var accountId = User.GetAccountId();
            if (accountId == null) return Unauthorized();

            if (User.IsInRole("Member"))
            {
                var member = _memberAccounts.GetMemberForAccount(accountId.Value);
                if (member == null)
                    return Conflict(new { message = "This account is not linked to a member profile." });

                if (member.BranchId == null)
                    return BadRequest(new { message = "Member has no branch assigned. Please contact the administrator to set a branch." });

                req.MemberId = member.MemberId;
                req.BranchId = member.BranchId.Value;
            }
            else if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // The authenticated account is the actor; never trust an account ID sent by the browser.
            req.AccountId = accountId.Value;
            var (success, message, registrationId, discountAmount) = _service.Register(req);
            if (!success) return BadRequest(new { message });
            return Ok(new { message, registrationId, discountAmount });
        }

        [HttpPost("pause")]
        [Authorize(Roles = "Admin")]
        public IActionResult Pause([FromBody] PausePackageRequest req)
        {
            _service.Pause(req);
            return Ok(new { message = "Package paused" });
        }

        [HttpPost("resume/{pauseId}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Resume(int pauseId)
        {
            _service.Resume(pauseId);
            return Ok(new { message = "Package resumed" });
        }

        // Danh sách gói đang sử dụng của 1 hội viên (cho Admin chọn gói cần hủy).
        // Gồm cả gói đã hủy để hiển thị lịch sử — Status trả về từ DB.
        [HttpGet("member/{memberId}")]
        [Authorize(Roles = "Admin")]
        public IActionResult MemberPackages(int memberId)
        {
            return Ok(new { items = _memberAccounts.GetMyPackages(memberId) });
        }

        // Admin hủy gói của hội viên: ghi nhận Status = "Đã hủy" + CancelDate + CancelReason.
        // KHÔNG xóa Payments, KHÔNG hoàn tiền.
        [HttpPost("cancel")]
        [Authorize(Roles = "Admin")]
        public IActionResult Cancel([FromBody] CancelPackageRequest req)
        {
            var (success, message) = _service.CancelRegistration(req);
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }
    }
}
