namespace GymManagement.Api.DTOs
{
    public class GymClassRequest
    {
        public int? BranchId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? TrainerId { get; set; }
        public int Capacity { get; set; } = 20;
    }

    public class ClassEnrollmentRequest
    {
        public int MemberId { get; set; }
        public int ClassId { get; set; }
    }
}
