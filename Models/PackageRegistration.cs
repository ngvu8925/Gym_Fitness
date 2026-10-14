using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagement.Api.Models
{
    [Table("PackageRegistrations")]
    public class PackageRegistration
    {
        [Column("RegistrationId")]
        public int RegistrationId { get; set; }

        [Column("MemberId")]
        public int MemberId { get; set; }

        [Column("PackageId")]
        public int PackageId { get; set; }

        [Column("BranchId")]
        public int? BranchId { get; set; }

        [Column("AccountId")]
        public int? AccountId { get; set; }

        [Column("PromotionId")]
        public int? PromotionId { get; set; }

        [Column("DiscountAmount")]
        public decimal DiscountAmount { get; set; }

        [Column("StartDate")]
        public DateTime StartDate { get; set; }

        [Column("EndDate")]
        public DateTime EndDate { get; set; }

        [Column("Status")]
        public string Status { get; set; } = string.Empty;
    }
}
