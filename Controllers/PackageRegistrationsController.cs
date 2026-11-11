using GymManagement.Api.DTOs;
using GymManagement.Api.Services;
using System.Security.Claims;
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
            if (!TryGetAccountId(out var accountId)) return Unauthorized();

            if (User.IsInRole("Member"))
            {
                var member = _memberAccounts.GetMemberForAccount(accountId);
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
            req.AccountId = accountId;
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

        private bool TryGetAccountId(out int accountId)
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("AccountId");
            return int.TryParse(value, out accountId);
        }
    }
}
