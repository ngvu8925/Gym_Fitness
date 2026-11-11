using GymManagement.Api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Api.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    [Authorize(Roles = "Admin")]
    public class MemberAccountsController : ControllerBase
    {
        private readonly MemberAccountRepository _repo;

        public MemberAccountsController(MemberAccountRepository repo) => _repo = repo;

        [HttpGet("member-options")]
        public IActionResult MemberOptions() => Ok(_repo.MemberOptions().ToJsonList());
    }
}
