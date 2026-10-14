using GymManagement.Api.DTOs;
using GymManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Api.Controllers
{
    [ApiController]
    [Route("api/members")]
    public class MembersController : ControllerBase
    {
        private readonly MemberService _service;

        public MembersController(MemberService service) => _service = service;

        [HttpGet]
        public IActionResult Search([FromQuery] string? keyword) => Ok(_service.Search(keyword).ToJsonList());

        [HttpPost]
        public IActionResult Add([FromBody] MemberRequest req)
        {
            var (success, message, memberId) = _service.Add(req);
            if (!success) return BadRequest(new { message });
            return Ok(new { message, memberId });
        }

        [HttpPut("{memberId}")]
        public IActionResult Update(int memberId, [FromBody] MemberRequest req)
        {
            var (success, message) = _service.Update(memberId, req);
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }

        [HttpDelete("{memberId}")]
        public IActionResult Delete(int memberId)
        {
            _service.Delete(memberId);
            return Ok(new { message = "Member deactivated" });
        }
    }
}
