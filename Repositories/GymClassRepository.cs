using System.Data;
using GymManagement.Api.DTOs;
using Microsoft.Data.SqlClient;

namespace GymManagement.Api.Repositories
{
    public class GymClassRepository
    {
        private readonly DatabaseHelper _db;

        public GymClassRepository(DatabaseHelper db) => _db = db;

        public DataTable List() => _db.ExecuteQuery("sp_Class_GetAll");

        public int Add(GymClassRequest req)
        {
            var ps = new SqlParameter[]
            {
                new("@BranchId", (object?)req.BranchId ?? DBNull.Value),
                new("@ClassName", req.Name),
                new("@TrainerId", (object?)req.TrainerId ?? DBNull.Value),
                new("@Capacity", req.Capacity)
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
                new("@Capacity", req.Capacity)
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
