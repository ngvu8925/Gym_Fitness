using System.Security.Cryptography;
using System.Text;
using GymManagement.Api.Services;

namespace GymManagement.Api.Tests
{
    public class PasswordHelperTests
    {
        private static string Sha256Hex(string input)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        [Fact]
        public void Hash_TraVeHashBcryptCoTheVerify()
        {
            var hash = PasswordHelper.Hash("MatKhau@123");

            Assert.StartsWith("$2", hash);
            Assert.True(PasswordHelper.Verify("MatKhau@123", hash));
            Assert.False(PasswordHelper.Verify("SaiMatKhau", hash));
        }

        [Fact]
        public void Verify_HashRongHoacSaiDinhDang_TraVeFalse()
        {
            Assert.False(PasswordHelper.Verify("abc", ""));
            Assert.False(PasswordHelper.Verify("abc", "not-a-bcrypt-hash"));
        }

        [Fact]
        public void IsLegacyHash_NhanDienSha256Hex64KyTu()
        {
            var legacy = Sha256Hex("Admin@123");

            Assert.Equal(64, legacy.Length);
            Assert.True(PasswordHelper.IsLegacyHash(legacy));
            Assert.False(PasswordHelper.IsLegacyHash(PasswordHelper.Hash("Admin@123")));
            Assert.True(PasswordHelper.VerifyLegacy("Admin@123", legacy));
            Assert.False(PasswordHelper.VerifyLegacy("Sai", legacy));
        }
    }
}
