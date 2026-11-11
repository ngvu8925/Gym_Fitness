using GymManagement.Api.DTOs;
using GymManagement.Api.DTOs.Common;
using GymManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Api.Controllers
{
    [ApiController]
    [Route("api/packages")]
    public class PackagesController : ControllerBase
    {
        private readonly IPackageService _service;

        public PackagesController(IPackageService service) => _service = service;

        [HttpGet]
        public IActionResult List([FromQuery] UserQueryParameters query) => Ok(_service.List(query));

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Add([FromBody] PackageRequest req)
        {
            var (success, message) = _service.Add(req);
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }

        [HttpPut("{packageId}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Update(int packageId, [FromBody] PackageRequest req)
        {
            var (success, message) = _service.Update(packageId, req);
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }

        [HttpDelete("{packageId}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int packageId)
        {
            _service.Delete(packageId);
            return Ok(new { message = "Package deactivated" });
        }

        [HttpGet("expiring-soon")]
        [Authorize(Roles = "Admin")]
        public IActionResult ExpiringSoon([FromQuery] int days = 7) => Ok(_service.ExpiringSoon(days));
    }
}
