namespace GymManagement.Api.DTOs
{
    public class PausePackageRequest
    {
        public int RegistrationId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
