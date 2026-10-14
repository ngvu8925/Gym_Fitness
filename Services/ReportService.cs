using System.Data;
using GymManagement.Api.Repositories;

namespace GymManagement.Api.Services
{
    public class ReportService
    {
        private readonly ReportRepository _repo;

        public ReportService(ReportRepository repo) => _repo = repo;
        public ReportService() : this(new ReportRepository()) { }

        public DataTable MonthlyRevenue(int year, int month) => _repo.MonthlyRevenue(year, month);

        public DataTable AllMonthlyRevenue() => _repo.AllMonthlyRevenue();

        public DataTable BestSellingPackages() => _repo.BestSellingPackages();

        public DataTable BranchRevenue() => _repo.BranchRevenue();
    }
}
