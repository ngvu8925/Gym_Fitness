
namespace GymManagement.Api.Models
{
    public class Trainer
    {
        public int TrainerId { get; set; }
        public int? BranchId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Specialty { get; set; }
        public string? Phone { get; set; }
        public DateTime? HireDate { get; set; }
        public bool Status { get; set; }
    }
}
