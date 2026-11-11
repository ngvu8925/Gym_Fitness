
namespace GymManagement.Api.Models
{
    public class GymClass
    {
        public int ClassId { get; set; }
        public int? BranchId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? TrainerId { get; set; }
        public int Capacity { get; set; }
        public bool Status { get; set; }
    }
}
