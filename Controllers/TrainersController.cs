using GymManagement.Api.DTOs;
using GymManagement.Api.DTOs.Common;
using GymManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Api.Controllers
{
    [ApiController]
    [Route("api/trainers")]
    public class TrainersController : ControllerBase
    {
        private readonly ITrainerService _service;

        public TrainersController(ITrainerService service) => _service = service;

        [HttpGet]
        public IActionResult List([FromQuery] UserQueryParameters query) => Ok(_service.List(query));

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Add([FromBody] TrainerRequest req)
        {
            var (success, message, trainerId) = _service.Add(req);
            if (!success) return BadRequest(new { message });
            return Ok(new { message, trainerId });
        }

        [HttpPut("{trainerId}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Update(int trainerId, [FromBody] TrainerRequest req)
        {
            var (success, message) = _service.Update(trainerId, req);
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }

        [HttpDelete("{trainerId}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int trainerId)
        {
            _service.Delete(trainerId);
            return Ok(new { message = "Trainer deactivated" });
        }
    }
}
