using System.Security.Claims;
using GymManagement.Api.Data;
using GymManagement.Api.DTOs;
using GymManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Api.Controllers
{
    [ApiController]
    [Route("api/member-portal")]
    [Authorize(Roles = "Member")]
    public class MemberPortalController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly MemberAccountService _memberAccounts;
        private readonly MemberService _members;

        public MemberPortalController(AppDbContext db, MemberAccountService memberAccounts, MemberService members)
        {
            _db = db;
            _memberAccounts = memberAccounts;
            _members = members;
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            if (!TryGetAccountId(out var accountId)) return Unauthorized();

            var member = await _memberAccounts.GetMemberForAccountAsync(accountId);
            if (member == null)
                return Conflict(new { message = "This account is not linked to a member profile. Please contact the administrator." });

            var registrations = await (
                from registration in _db.PackageRegistrations.AsNoTracking()
                join package in _db.MembershipPackages.AsNoTracking()
                    on registration.PackageId equals package.PackageId
                where registration.MemberId == member.MemberId
                orderby registration.StartDate descending, registration.RegistrationId descending
                select new
                {
                    registrationId = registration.RegistrationId,
                    packageId = package.PackageId,
                    packageName = package.Name,
                    packageType = package.Type,
                    months = package.Months,
                    listPrice = package.Price,
                    discountAmount = registration.DiscountAmount,
                    paidPrice = package.Price > registration.DiscountAmount
                        ? package.Price - registration.DiscountAmount
                        : 0,
                    startDate = registration.StartDate,
                    endDate = registration.EndDate,
                    status = registration.Status
                }).ToListAsync();

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
                packages = registrations
            });
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] MemberRequest request)
        {
            if (!TryGetAccountId(out var accountId)) return Unauthorized();
            var member = await _memberAccounts.GetMemberForAccountAsync(accountId);
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
