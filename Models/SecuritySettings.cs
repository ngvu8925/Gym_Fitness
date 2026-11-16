namespace GymManagement.Api.Models
{
    /// <summary>
    /// Cấu hình bảo mật đăng nhập (Task 06). Giá trị override được qua
    /// appsettings / user secrets: SecuritySettings:MaxFailedAttempts ...
    /// </summary>
    public class SecuritySettings
    {
        /// <summary>Số lần đăng nhập sai tối đa trước khi khóa (mặc định 5).</summary>
        public int MaxFailedAttempts { get; set; } = 5;

        /// <summary>Thời gian khóa tài khoản sau khi vượt ngưỡng, tính bằng phút (mặc định 15).</summary>
        public int LockoutMinutes { get; set; } = 15;

        /// <summary>Rate limit: số request đăng nhập tối đa mỗi phút cho 1 IP (mặc định 10).</summary>
        public int LoginRateLimitPerMinutePerIp { get; set; } = 10;

        /// <summary>Rate limit: số request đăng nhập tối đa mỗi phút cho 1 username (mặc định 5).</summary>
        public int LoginRateLimitPerMinutePerUsername { get; set; } = 5;
    }
}
