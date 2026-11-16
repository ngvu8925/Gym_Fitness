namespace GymManagement.Api.DTOs
{
    /// <summary>
    /// Body cho POST /api/auth/change-password.
    /// AccountId lấy từ JWT claims — KHÔNG nhận từ client.
    /// </summary>
    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
