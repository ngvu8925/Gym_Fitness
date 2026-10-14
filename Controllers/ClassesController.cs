using GymManagement.Api.DTOs;
using GymManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Api.Controllers
{
    [ApiController]
    [Route("api/classes")]
    public class ClassesController : ControllerBase
    {
        private readonly GymClassService _service;

        public ClassesController(GymClassService service) => _service = service;

        [HttpGet]
        public IActionResult List() => Ok(_service.List().ToJsonList());

        [HttpPost]
        public IActionResult Add([FromBody] GymClassRequest req)
        {
            var (success, message, classId) = _service.Add(req);
            if (!success) return BadRequest(new { message });
            return Ok(new { message, classId });
        }

        [HttpPost("enroll")]
        public IActionResult Enroll([FromBody] ClassEnrollmentRequest req)
        {
            var (success, message, enrollmentId) = _service.Enroll(req);
            if (!success) return BadRequest(new { message });
            return Ok(new { message, enrollmentId });
        }
    }
}
