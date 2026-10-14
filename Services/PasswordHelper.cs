using System.Security.Cryptography;
using System.Text;

namespace GymManagement.Api.Services
{
    public static class PasswordHelper
    {
        public static string Hash(string plainPassword)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plainPassword));
            var sb = new StringBuilder();
            foreach (byte b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}