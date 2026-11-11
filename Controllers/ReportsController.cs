using GymManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Api.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Authorize(Roles = "Admin")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _service;

        public ReportsController(IReportService service) => _service = service;

        [HttpGet("monthly-revenue")]
        public IActionResult MonthlyRevenue([FromQuery] int year, [FromQuery] int month)
            => Ok(_service.MonthlyRevenue(year, month));

        [HttpGet("all-monthly-revenue")]
        public IActionResult AllMonthlyRevenue() => Ok(_service.AllMonthlyRevenue());

        [HttpGet("best-selling-packages")]
        public IActionResult BestSellingPackages() => Ok(_service.BestSellingPackages());

        [HttpGet("branch-revenue")]
        public IActionResult BranchRevenue() => Ok(_service.BranchRevenue());
    }
}
