using GymManagement.Api.DTOs;
using GymManagement.Api.DTOs.Common;
using GymManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Api.Controllers
{
    [ApiController]
    [Route("api/members")]
    public class MembersController : ControllerBase
    {
        private readonly IMemberService _service;
        private readonly IMemberAccountService _memberAccounts;

        public MembersController(IMemberService service, IMemberAccountService memberAccounts)
        {
            _service = service;
            _memberAccounts = memberAccounts;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Search([FromQuery] UserQueryParameters query) => Ok(_service.Search(query.Keyword, query));

        [HttpGet("{memberId}")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetById(int memberId)
        {
            var member = _service.GetById(memberId);
            if (member == null) return NotFound(new { message = "Member not found" });
            return Ok(member);
        }

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
        public IActionResult LinkAccount(int memberId, [FromBody] LinkMemberAccountRequest req)
        {
            var (success, message) = _memberAccounts.LinkAccount(memberId, req.AccountId);
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }
    }
            
    public class LinkMemberAccountRequest
    {
        public int? AccountId { get; set; }
    }
}
