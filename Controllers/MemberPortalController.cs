using GymManagement.Api.Common;
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
        private readonly IMemberAccountService _memberAccounts;
        private readonly IMemberService _members;

        public MemberPortalController(IMemberAccountService memberAccounts, IMemberService members)
        {
            _memberAccounts = memberAccounts;
            _members = members;
        }

        [HttpGet("me")]
        public IActionResult Me()
        {
            var accountId = User.GetAccountId();
            if (accountId == null) return Unauthorized();

            var member = _memberAccounts.GetMemberForAccount(accountId.Value);
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
            var accountId = User.GetAccountId();
            if (accountId == null) return Unauthorized();
            var member = _memberAccounts.GetMemberForAccount(accountId.Value);
            if (member == null)
                return Conflict(new { message = "This account is not linked to a member profile." });

            request.BranchId = member.BranchId;
            var (success, message) = _members.Update(member.MemberId, request);
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }
    }
}
