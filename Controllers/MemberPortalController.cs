using System.Data;
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

            var registrations = _memberAccounts.GetMyRegistrations(member.MemberId);

            var packages = new List<object>();
            foreach (DataRow row in registrations.Rows)
            {
                decimal listPrice = row["UnitPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(row["UnitPrice"]);
                decimal discount = row["DiscountAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(row["DiscountAmount"]);
                decimal paidPrice = listPrice > discount ? listPrice - discount : 0;

                packages.Add(new
                {
                    registrationId = Convert.ToInt32(row["RegistrationId"]),
                    packageId = Convert.ToInt32(row["PackageId"]),
                    packageName = row["PackageName"]?.ToString(),
                    packageType = row["PackageType"]?.ToString(),
                    months = row["DurationInMonths"] == DBNull.Value ? 0 : Convert.ToInt32(row["DurationInMonths"]),
                    listPrice,
                    discountAmount = discount,
                    paidPrice,
                    startDate = row["StartDate"],
                    endDate = row["EndDate"],
                    status = row["Status"]?.ToString()
                });
            }

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
                packages
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
