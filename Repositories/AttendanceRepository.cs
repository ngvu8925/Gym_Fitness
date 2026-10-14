using Microsoft.Data.SqlClient;

namespace GymManagement.Api.Repositories
{
    public class AttendanceRepository
    {
        private readonly DatabaseHelper _db;

        public AttendanceRepository(DatabaseHelper db) => _db = db;
        public AttendanceRepository() : this(new DatabaseHelper()) { }

        public void CheckIn(int memberId)
        {
            _db.ExecuteNonQuery("sp_Attendance_CheckIn", new SqlParameter("@MemberId", memberId));
        }

        public void CheckOut(int attendanceId)
        {
            _db.ExecuteNonQuery("sp_Attendance_CheckOut", new SqlParameter("@AttendanceId", attendanceId));
        }
    }
}
