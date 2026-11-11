namespace GymManagement.Api.DTOs
{
    /// <summary>Kết quả xác thực trả về từ AccountService (không lộ schema DB).</summary>
    public class LoginResult
    {
        public int AccountId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public int? BranchId { get; set; }
    }
}
