using GymManagement.Api.DTOs;
using GymManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Api.Controllers
{
    [ApiController]
    [Route("api/trainers")]
    public class TrainersController : ControllerBase
    {
        private readonly TrainerService _service;

        public TrainersController(TrainerService service) => _service = service;

        [HttpGet]
        public IActionResult List() => Ok(_service.List().ToJsonList());

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Add([FromBody] TrainerRequest req)
        {
            var (success, message, trainerId) = _service.Add(req);
            if (!success) return BadRequest(new { message });
            return Ok(new { message, trainerId });
        }
    }
}
