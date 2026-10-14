namespace GymManagement.Api.DTOs
{
    public class TrainerRequest
    {
        public int? BranchId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Specialty { get; set; }
        public string? Phone { get; set; }
        public DateTime? HireDate { get; set; }
    }
}
