using GymManagement.Api.Repositories;

namespace GymManagement.Api.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _repo;

        public AttendanceService(IAttendanceRepository repo) => _repo = repo;

        public (bool Success, string Message) CheckIn(int memberId)
        {
            // Lỗi DB (SP văng exception) để pipeline ExceptionHandlingMiddleware xử lý thống nhất.
            _repo.CheckIn(memberId);
            return (true, "Check-in successful");
        }

        public void CheckOut(int attendanceId) => _repo.CheckOut(attendanceId);
    }
}
