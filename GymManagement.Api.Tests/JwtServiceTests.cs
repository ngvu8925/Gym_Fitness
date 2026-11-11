using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GymManagement.Api.Models;
using GymManagement.Api.Services;
using Microsoft.Extensions.Options;

namespace GymManagement.Api.Tests
{
    public class JwtServiceTests
    {
        private static JwtService CreateService()
        {
            var settings = new JwtSettings
            {
                SecretKey = "test-secret-key-must-be-long-enough-1234567890",
                Issuer = "https://test-issuer",
                Audience = "https://test-audience",
                ExpiryMinutes = 30
            };
            return new JwtService(Options.Create(settings));
        }

        [Fact]
        public void GenerateToken_TraVeJwtHopLe_DungClaimsVaIssuer()
        {
            var service = CreateService();
            var token = service.GenerateToken(accountId: 42, username: "testuser", role: "Member", branchId: 2);

            var handler = new JwtSecurityTokenHandler();
            Assert.True(handler.CanReadToken(token));

            var jwt = handler.ReadJwtToken(token);
            Assert.Equal("42", jwt.Claims.First(c => c.Type == "nameid").Value);
            Assert.Equal("testuser", jwt.Claims.First(c => c.Type == "unique_name").Value);
            Assert.Equal("Member", jwt.Claims.First(c => c.Type == "role").Value);
            Assert.Equal("42", jwt.Claims.First(c => c.Type == "AccountId").Value);
            Assert.Equal("2", jwt.Claims.First(c => c.Type == "BranchId").Value);
            Assert.Equal("https://test-issuer", jwt.Issuer);
            Assert.Equal("https://test-audience", jwt.Audiences.Single());
        }

        [Fact]
        public void GenerateToken_KhongCoBranchId_KhongCoClaimBranchId()
        {
            var service = CreateService();
            var token = service.GenerateToken(1, "admin", "Admin", branchId: null);

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            Assert.DoesNotContain(jwt.Claims, c => c.Type == "BranchId");
        }

        [Fact]
        public void GenerateToken_TokenHetHanTheoExpiryMinutes()
        {
            var service = CreateService();
            var before = DateTime.UtcNow.AddSeconds(-5);
            var token = service.GenerateToken(1, "u", "Member", null);

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            Assert.True(jwt.ValidTo > DateTime.UtcNow.AddMinutes(29));
            Assert.True(jwt.ValidTo < DateTime.UtcNow.AddMinutes(31) || before < jwt.ValidTo);
        }
    }
}
