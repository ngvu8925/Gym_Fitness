
namespace GymManagement.Api.Models
{
    public class Attendance
    {
        public int AttendanceId { get; set; }
        public int MemberId { get; set; }
        public int? BranchId { get; set; }
        public DateTime CheckInAt { get; set; }
        public DateTime? CheckOutAt { get; set; }
    }
}
