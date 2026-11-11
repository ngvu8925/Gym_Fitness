using System.Security.Claims;
using GymManagement.Api.DTOs;
using GymManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Api.Controllers
{
    [ApiController]
    [Route("api/member-portal")]
    [Authorize(Roles = "Member")]
    public class MemberPortalController : ControllerBase
    {
        private readonly MemberAccountService _memberAccounts;
        private readonly MemberService _members;

        public MemberPortalController(MemberAccountService memberAccounts, MemberService members)
        {
            _memberAccounts = memberAccounts;
            _members = members;
        }

        [HttpGet("me")]
        public IActionResult Me()
        {
            if (!TryGetAccountId(out var accountId)) return Unauthorized();

            var member = _memberAccounts.GetMemberForAccount(accountId);
            if (member == null)
                return Conflict(new { message = "This account is not linked to a member profile. Please contact the administrator." });

            return Ok(new
            {
                member = new
                {
                    member.MemberId,
                    member.BranchId,
                    member.FullName,
                    member.Gender,
                    member.DateOfBirth,
                    member.Phone,
                    member.Email,
                    member.Address,
                    member.Status
                },
                packages = _memberAccounts.GetMyPackages(member.MemberId)
            });
        }

        [HttpPut("me")]
        public IActionResult UpdateMyProfile([FromBody] MemberRequest request)
        {
            if (!TryGetAccountId(out var accountId)) return Unauthorized();
            var member = _memberAccounts.GetMemberForAccount(accountId);
            if (member == null)
                return Conflict(new { message = "This account is not linked to a member profile." });

            request.BranchId = member.BranchId;
            var (success, message) = _members.Update(member.MemberId, request);
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }

        private bool TryGetAccountId(out int accountId)
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("AccountId");
            return int.TryParse(value, out accountId);
        }
    }
}
