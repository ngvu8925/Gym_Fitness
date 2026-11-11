
namespace GymManagement.Api.Models
{
    public class MembershipPackage
    {
        public int PackageId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Months { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public bool Status { get; set; }
    }
}
