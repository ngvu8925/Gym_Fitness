namespace GymManagement.Api.DTOs
{
    /// <summary>
    /// Body cho POST /api/package-registrations/cancel (Admin hủy gói của hội viên).
    /// </summary>
    public class CancelPackageRequest
    {
        public int RegistrationId { get; set; }
        public string? Reason { get; set; }
    }
}
