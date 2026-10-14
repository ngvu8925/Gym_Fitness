using System.Data;
using Microsoft.Data.SqlClient;

namespace GymManagement.Api.Repositories
{
    public class ReportRepository
    {
        private readonly DatabaseHelper _db;

        public ReportRepository(DatabaseHelper db) => _db = db;
        public ReportRepository() : this(new DatabaseHelper()) { }

        public DataTable MonthlyRevenue(int year, int month)
        {
            var ps = new SqlParameter[]
            {
                new("@Year", year),
                new("@Month", month)
            };
            return _db.ExecuteQuery("sp_Report_MonthlyRevenue", ps);
        }

        public DataTable AllMonthlyRevenue() => _db.ExecuteQuery("sp_Report_AllMonthsRevenue");

        public DataTable BestSellingPackages() => _db.ExecuteQuery("sp_Report_BestSellingPackages");

        public DataTable BranchRevenue() => _db.ExecuteQuery("sp_Report_RevenueByBranch");
    }
}
