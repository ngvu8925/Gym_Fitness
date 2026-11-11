using System.Data;
using GymManagement.Api.DTOs;
using Microsoft.Data.SqlClient;

namespace GymManagement.Api.Repositories
{
    public class GymClassRepository : IGymClassRepository
    {
        private readonly DatabaseHelper _db;

        public GymClassRepository(DatabaseHelper db) => _db = db;

        public DataTable List(DTOs.Common.UserQueryParameters query)
        {
            var ps = new SqlParameter[]
            {
                new("@Keyword", query.NormalizedKeyword ?? (object)DBNull.Value),
                new("@Page", query.Page),
                new("@PageSize", query.PageSize),
                new("@SortDir", query.IsDescending ? "DESC" : "ASC")
            };
            return PagedQuery.Execute(_db, "sp_Class_GetAll", ps,
                legacyCall: () => _db.ExecuteQuery("sp_Class_GetAll"));
        }

        public int Add(GymClassRequest req)
        {
            var ps = new SqlParameter[]
            {
                new("@BranchId", (object?)req.BranchId ?? DBNull.Value),
                new("@ClassName", req.Name),
                new("@TrainerId", (object?)req.TrainerId ?? DBNull.Value),
                new("@Capacity", req.Capacity),
                new("@DaysOfWeek", (object?)req.DaysOfWeek ?? DBNull.Value),
                new("@StartTime", (object?)req.StartTime ?? DBNull.Value),
                new("@EndTime", (object?)req.EndTime ?? DBNull.Value)
            };
            var dt = _db.ExecuteQuery("sp_Class_Create", ps);
            return Convert.ToInt32(dt.Rows[0]["ClassId"]);
        }

        public void Update(int classId, GymClassRequest req)
        {
            var ps = new SqlParameter[]
            {
                new("@ClassId", classId),
                new("@BranchId", (object?)req.BranchId ?? DBNull.Value),
                new("@ClassName", req.Name),
                new("@TrainerId", (object?)req.TrainerId ?? DBNull.Value),
                new("@Capacity", req.Capacity),
                new("@DaysOfWeek", (object?)req.DaysOfWeek ?? DBNull.Value),
                new("@StartTime", (object?)req.StartTime ?? DBNull.Value),
                new("@EndTime", (object?)req.EndTime ?? DBNull.Value)
            };
            _db.ExecuteNonQuery("sp_Class_Update", ps);
        }

        public void Delete(int classId)
        {
            _db.ExecuteNonQuery("sp_Class_Delete", new SqlParameter("@ClassId", classId));
        }

        public int Enroll(ClassEnrollmentRequest req)
        {
            var ps = new SqlParameter[]
            {
                new("@MemberId", req.MemberId),
                new("@ClassId", req.ClassId)
            };
            var dt = _db.ExecuteQuery("sp_ClassRegistration_Create", ps);
            return Convert.ToInt32(dt.Rows[0]["ClassRegistrationId"]);
        }
    }
}
