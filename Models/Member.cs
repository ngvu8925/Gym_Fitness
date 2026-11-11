
namespace GymManagement.Api.Models
{
    public class Member
    {
        public int MemberId { get; set; }
        public int? BranchId { get; set; }
        public int? AccountId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Avatar { get; set; }
        public int? ReferrerId { get; set; }
        public DateTime JoinDate { get; set; }
        public bool Status { get; set; }
    }
}
