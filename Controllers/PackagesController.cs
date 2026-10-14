using GymManagement.Api.DTOs;
using GymManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Api.Controllers
{
    [ApiController]
    [Route("api/packages")]
    public class PackagesController : ControllerBase
    {
        private readonly PackageService _service;

        public PackagesController(PackageService service) => _service = service;

        [HttpGet]
        public IActionResult List() => Ok(_service.List().ToJsonList());

        [HttpPost]
        public IActionResult Add([FromBody] PackageRequest req)
        {
            var (success, message) = _service.Add(req);
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }

        [HttpGet("expiring-soon")]
        public IActionResult ExpiringSoon([FromQuery] int days = 7) => Ok(_service.ExpiringSoon(days).ToJsonList());
    }
}
