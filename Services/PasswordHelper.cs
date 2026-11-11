using System.Security.Cryptography;
using System.Text;

namespace GymManagement.Api.Services
{
    public static class PasswordHelper
    {
        private const int BcryptWorkFactor = 11;

        /// <summary>
        /// Hash mật khẩu bằng BCrypt (salt tự sinh, lưu kèm trong chuỗi hash).
        /// </summary>
        public static string Hash(string plainPassword)
        {
            return BCrypt.Net.BCrypt.HashPassword(plainPassword, BcryptWorkFactor);
        }

        /// <summary>
        /// Kiểm tra mật khẩu nhập vào khớp với hash BCrypt đã lưu.
        /// </summary>
        public static bool Verify(string plainPassword, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash)) return false;

            try
            {
                return BCrypt.Net.BCrypt.Verify(plainPassword, storedHash);
            }
            catch (BCrypt.Net.HashInformationException)
            {
                return false;
            }
        }

        /// <summary>
        /// Hash cũ dạng SHA-256 không salt (64 ký tự hex) — chỉ dùng để
        /// kiểm tra tài khoản legacy và nâng cấp dần sang BCrypt.
        /// </summary>
        public static bool IsLegacyHash(string storedHash)
        {
            return storedHash?.Length == 64
                && storedHash.All(c => Uri.IsHexDigit(c));
        }

        public static bool VerifyLegacy(string plainPassword, string storedHash)
        {
            return string.Equals(HashLegacy(plainPassword), storedHash, StringComparison.OrdinalIgnoreCase);
        }

        private static string HashLegacy(string plainPassword)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plainPassword));
            var sb = new StringBuilder();
            foreach (byte b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
