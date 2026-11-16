using System.Collections.Concurrent;
using GymManagement.Api.Models;
using Microsoft.Extensions.Options;

namespace GymManagement.Api.Common
{
    /// <summary>
    /// Rate limiting theo TÀI KHOẢN cho endpoint đăng nhập (Task 06).
    /// Giới hạn theo IP dùng middleware RateLimiter (Program.cs); ở đây giới hạn
    /// theo username để chặn brute-force một tài khoản từ nhiều IP khác nhau,
    /// mà không khóa nhầm người hợp lệ dùng chung IP (ngưỡng riêng cho username).
    /// </summary>
    public class LoginThrottle
    {
        private readonly ConcurrentDictionary<string, (DateTime WindowStart, int Count)> _attempts = new();
        private readonly int _limitPerMinute;

        public LoginThrottle(IOptions<SecuritySettings> options)
        {
            _limitPerMinute = Math.Max(1, options.Value.LoginRateLimitPerMinutePerUsername);
        }

        /// <summary>
        /// Ghi nhận 1 request đăng nhập cho username; trả true nếu vượt giới hạn
        /// (cửa sổ 1 phút, tự reset sang cửa sổ mới).
        /// </summary>
        public bool IsThrottled(string username)
        {
            var key = username.Trim().ToLowerInvariant();
            if (key.Length == 0) return false;

            var now = DateTime.UtcNow;
            var (_, throttled) = _attempts.AddOrUpdate(
                key,
                _ => (now, 1),
                (_, old) =>
                {
                    var (windowStart, count) = old;
                    if (now - windowStart > TimeSpan.FromMinutes(1))
                        return (now, 1); // cửa sổ mới
                    return (windowStart, count + 1);
                });

            var entry = _attempts[key];
            return entry.Count > _limitPerMinute;
        }
    }
}
