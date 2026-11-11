using GymManagement.Api.Repositories;

namespace GymManagement.Api.Services
{
    public class AttendanceService
    {
        private readonly AttendanceRepository _repo;

        public AttendanceService(AttendanceRepository repo) => _repo = repo;

        public (bool Success, string Message) CheckIn(int memberId)
        {
            try
            {
                _repo.CheckIn(memberId);
                return (true, "Check-in successful");
            }
            catch (Exception ex)
            {
                return (false, "Error: " + ex.Message);
            }
        }

        public void CheckOut(int attendanceId) => _repo.CheckOut(attendanceId);
    }
}
