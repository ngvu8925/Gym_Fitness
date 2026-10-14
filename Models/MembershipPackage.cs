using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagement.Api.Models
{
    [Table("MembershipPackages")]
    public class MembershipPackage
    {
        [Column("PackageId")]
        public int PackageId { get; set; }

        [Column("PackageName")]
        public string Name { get; set; } = string.Empty;

        [Column("PackageType")]
        public string Type { get; set; } = string.Empty;

        [Column("DurationInMonths")]
        public int Months { get; set; }

        [Column("UnitPrice")]
        public decimal Price { get; set; }

        [Column("Description")]
        public string? Description { get; set; }

        [Column("Status")]
        public bool Status { get; set; }
    }
}
