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
        private readonly PackageService _service;
        private readonly MemberAccountService _memberAccounts;

        public PackageRegistrationsController(PackageService service, MemberAccountService memberAccounts)
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

                req.MemberId = member.MemberId;
                req.BranchId = member.BranchId ?? 1;
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
    }
}
