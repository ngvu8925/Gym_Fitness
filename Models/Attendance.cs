using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagement.Api.Models
{
    [Table("Attendances")]
    public class Attendance
    {
        [Column("AttendanceId")]
        public int AttendanceId { get; set; }

        [Column("MemberId")]
        public int MemberId { get; set; }

        [Column("BranchId")]
        public int? BranchId { get; set; }

        [Column("CheckInTime")]
        public DateTime CheckInAt { get; set; }

        [Column("CheckOutTime")]
        public DateTime? CheckOutAt { get; set; }
    }
}
