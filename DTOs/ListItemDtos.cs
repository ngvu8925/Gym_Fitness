namespace GymManagement.Api.DTOs
{
    /// <summary>Danh sách hội viên (sp_Member_Search).</summary>
    public class MemberListItem
    {
        public int MemberId { get; set; }
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Avatar { get; set; }
        public int? ReferrerId { get; set; }
        public DateTime? JoinDate { get; set; }
        public bool? Status { get; set; }
    }

    /// <summary>Gói tập (sp_MembershipPackage_GetAll).</summary>
    public class PackageListItem
    {
        public int PackageId { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public string? PackageType { get; set; }
        public int Months { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public bool? Status { get; set; }
    }

    /// <summary>Lớp học (sp_Class_GetAll).</summary>
    public class ClassListItem
    {
        public int ClassId { get; set; }
        public int? BranchId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int? TrainerId { get; set; }
        public string? TrainerName { get; set; }
        public int Capacity { get; set; }
        public bool? Status { get; set; }
    }

    /// <summary>Huấn luyện viên (sp_Trainer_GetAll).</summary>
    public class TrainerListItem
    {
        public int TrainerId { get; set; }
        public int? BranchId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Specialty { get; set; }
        public string? Phone { get; set; }
        public DateTime? HireDate { get; set; }
    }

    /// <summary>Hội viên sắp hết hạn gói (sp_Member_GetExpiringSoon).</summary>
    public class ExpiringMemberItem
    {
        public int MemberId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? PackageName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    /// <summary>Các báo cáo doanh thu (chung cho 4 SP report).</summary>
    public class ReportItem
    {
        public int? Month { get; set; }
        public int? Year { get; set; }
        public decimal? TotalRevenue { get; set; }
        public int? RegistrationCount { get; set; }
        public string? PackageName { get; set; }
        public decimal? Revenue { get; set; }
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }
    }

    /// <summary>Gói tập đã đăng ký của member trong Member Portal.</summary>
    public class MemberPackageItem
    {
        public int RegistrationId { get; set; }
        public int PackageId { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public string PackageType { get; set; } = string.Empty;
        public int Months { get; set; }
        public decimal ListPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal PaidPrice { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>Account role Member + memberId đã link (cho dropdown Admin).</summary>
    public class AccountMemberOptionItem
    {
        public int AccountId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public int? MemberId { get; set; }
    }
}
