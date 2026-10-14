using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagement.Api.Models
{
    [Table("Members")]
    public class Member
    {
        [Column("MemberId")]
        public int MemberId { get; set; }

        [Column("BranchId")]
        public int? BranchId { get; set; }

        [Column("FullName")]
        public string FullName { get; set; } = string.Empty;

        [Column("Gender")]
        public string? Gender { get; set; }

        [Column("DateOfBirth")]
        public DateTime? DateOfBirth { get; set; }

        [Column("PhoneNumber")]
        public string Phone { get; set; } = string.Empty;

        [Column("Email")]
        public string? Email { get; set; }

        [Column("Address")]
        public string? Address { get; set; }

        [Column("ProfileImage")]
        public string? Avatar { get; set; }

        [Column("ReferrerMemberId")]
        public int? ReferrerId { get; set; }

        [Column("JoinDate")]
        public DateTime JoinDate { get; set; }

        [Column("Status")]
        public bool Status { get; set; }
    }
}
