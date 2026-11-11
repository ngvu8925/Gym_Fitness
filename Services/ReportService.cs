using GymManagement.Api.DTOs;
using GymManagement.Api.Repositories;

namespace GymManagement.Api.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _repo;

        public ReportService(IReportRepository repo) => _repo = repo;

        public List<ReportItem> MonthlyRevenue(int year, int month) => Map(_repo.MonthlyRevenue(year, month));

        public List<ReportItem> AllMonthlyRevenue() => Map(_repo.AllMonthlyRevenue());

        public List<ReportItem> BestSellingPackages() => Map(_repo.BestSellingPackages());

        public List<ReportItem> BranchRevenue() => Map(_repo.BranchRevenue());

        private static List<ReportItem> Map(System.Data.DataTable dt)
        {
            return DtoMapper.MapList(dt, row => new ReportItem
            {
                Month = DtoMapper.GetNullableInt(row, "Month", "Thang"),
                Year = DtoMapper.GetNullableInt(row, "Year", "Nam"),
                TotalRevenue = DtoMapper.GetNullableDecimal(row, "TotalRevenue", "TongDoanhThu"),
                RegistrationCount = DtoMapper.GetNullableInt(row, "RegistrationCount", "SoLuotDangKy"),
                PackageName = DtoMapper.GetString(row, "PackageName", "TenGoi"),
                Revenue = DtoMapper.GetNullableDecimal(row, "Revenue", "DoanhThu"),
                BranchId = DtoMapper.GetNullableInt(row, "BranchId", "MaCN"),
                BranchName = DtoMapper.GetString(row, "BranchName", "TenCN")
            });
        }
    }
}
