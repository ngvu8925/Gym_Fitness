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

        /// <summary>
        /// Lấy thông tin 1 đăng ký lớp (chỉ khi đúng thuộc hội viên này).
        /// </summary>
        public DataTable GetEnrollment(int enrollmentId, int memberId)
        {
            return _db.ExecuteQuery(
                @"SELECT cr.ClassRegistrationId, cr.Status, c.DaysOfWeek, c.StartTime, c.EndTime
                  FROM dbo.ClassRegistrations cr
                  INNER JOIN dbo.Classes c ON c.ClassId = cr.ClassId
                  WHERE cr.ClassRegistrationId = @ClassRegistrationId AND cr.MemberId = @MemberId",
                new SqlParameter[]
                {
                    new("@ClassRegistrationId", enrollmentId),
                    new("@MemberId", memberId)
                },
                isStoredProcedure: false);
        }

        /// <summary>
        /// Hủy đăng ký lớp — SP chỉ cập nhật dòng đúng của hội viên, trả về số dòng ảnh hưởng.
        /// </summary>
        public int CancelRegistration(int enrollmentId, int memberId)
        {
            var dt = _db.ExecuteQuery("sp_ClassRegistration_Cancel", new SqlParameter[]
            {
                new("@ClassRegistrationId", enrollmentId),
                new("@MemberId", memberId)
            });
            return Convert.ToInt32(dt.Rows[0]["AffectedRows"]);
        }
    }
}
