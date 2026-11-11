using GymManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Api.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    [Authorize(Roles = "Admin")]
    public class MemberAccountsController : ControllerBase
    {
        private readonly IMemberAccountService _service;

        public MemberAccountsController(IMemberAccountService service) => _service = service;

        [HttpGet("member-options")]
        public IActionResult MemberOptions() => Ok(_service.MemberOptions());
    }
}
