using GymManagement.Api.DTOs;
using GymManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Api.Controllers
{
    [ApiController]
    [Route("api/package-registrations")]
    public class PackageRegistrationsController : ControllerBase
    {
        private readonly PackageService _service;

        public PackageRegistrationsController(PackageService service) => _service = service;

        [HttpPost]
        public IActionResult Register([FromBody] PackageRegistrationRequest req)
        {
            var (success, message, registrationId, discountAmount) = _service.Register(req);
            if (!success) return BadRequest(new { message });
            return Ok(new { message, registrationId, discountAmount });
        }

        [HttpPost("pause")]
        public IActionResult Pause([FromBody] PausePackageRequest req)
        {
            _service.Pause(req);
            return Ok(new { message = "Package paused" });
        }

        [HttpPost("resume/{pauseId}")]
        public IActionResult Resume(int pauseId)
        {
            _service.Resume(pauseId);
            return Ok(new { message = "Package resumed" });
        }
    }
}
