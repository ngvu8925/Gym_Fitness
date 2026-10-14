using GymManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Api.Controllers
{
    [ApiController]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly ReportService _service;

        public ReportsController(ReportService service) => _service = service;

        [HttpGet("monthly-revenue")]
        public IActionResult MonthlyRevenue([FromQuery] int year, [FromQuery] int month)
            => Ok(_service.MonthlyRevenue(year, month).ToJsonList());

        [HttpGet("all-monthly-revenue")]
        public IActionResult AllMonthlyRevenue() => Ok(_service.AllMonthlyRevenue().ToJsonList());

        [HttpGet("best-selling-packages")]
        public IActionResult BestSellingPackages() => Ok(_service.BestSellingPackages().ToJsonList());

        [HttpGet("branch-revenue")]
        public IActionResult BranchRevenue() => Ok(_service.BranchRevenue().ToJsonList());
    }
}
