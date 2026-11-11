using GymManagement.Api.DTOs;
using GymManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Api.Controllers
{
    [ApiController]
    [Route("api/members")]
    public class MembersController : ControllerBase
    {
        private readonly MemberService _service;
        private readonly MemberAccountService _memberAccounts;

        public MembersController(MemberService service, MemberAccountService memberAccounts)
        {
            _service = service;
            _memberAccounts = memberAccounts;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Search([FromQuery] string? keyword) => Ok(_service.Search(keyword).ToJsonList());

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Add([FromBody] MemberRequest req)
        {
            var (success, message, memberId) = _service.Add(req);
            if (!success) return BadRequest(new { message });
            return Ok(new { message, memberId });
        }

        [HttpPut("{memberId}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Update(int memberId, [FromBody] MemberRequest req)
        {
            var (success, message) = _service.Update(memberId, req);
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }

        [HttpDelete("{memberId}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int memberId)
        {
            _service.Delete(memberId);
            return Ok(new { message = "Member deactivated" });
        }

        [HttpPut("{memberId}/account")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> LinkAccount(int memberId, [FromBody] LinkMemberAccountRequest req)
        {
            var (success, message) = await _memberAccounts.LinkAccountAsync(memberId, req.AccountId);
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }
    }
            
    public class LinkMemberAccountRequest
    {
        public int? AccountId { get; set; }
    }
}
