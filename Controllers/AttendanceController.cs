using GymManagement.Api.DTOs;
using GymManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Api.Controllers
{
    [ApiController]
    [Route("api/attendance")]
    public class AttendanceController : ControllerBase
    {
        private readonly AttendanceService _service;

        public AttendanceController(AttendanceService service) => _service = service;

        [HttpPost("checkin")]
        public IActionResult CheckIn([FromBody] CheckInRequest req)
        {
            var (success, message) = _service.CheckIn(req.MemberId);
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }

        [HttpPost("checkout/{attendanceId}")]
        public IActionResult CheckOut(int attendanceId)
        {
            _service.CheckOut(attendanceId);
            return Ok(new { message = "Check-out successful" });
        }
    }
}
