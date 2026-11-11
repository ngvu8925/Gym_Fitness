
namespace GymManagement.Api.Models
{
    public class PackageRegistration
    {
        public int RegistrationId { get; set; }
        public int MemberId { get; set; }
        public int PackageId { get; set; }
        public int? BranchId { get; set; }
        public int? AccountId { get; set; }
        public int? PromotionId { get; set; }
        public decimal DiscountAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
