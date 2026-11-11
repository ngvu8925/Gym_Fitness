using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GymManagement.Api.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GymManagement.Api.Services
{
    public class JwtService
    {
        private readonly JwtSettings _jwtSettings;

        public JwtService(IOptions<JwtSettings> jwtOptions)
        {
            _jwtSettings = jwtOptions.Value;
        }

        public string GenerateToken(int accountId, string username, string role, int? branchId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
            var roleClaim = role.Trim().ToLowerInvariant() switch
            {
                "admin" => "Admin",
                "member" => "Member",
                _ => role.Trim()
            };

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, accountId.ToString()),
                new(ClaimTypes.Name, username),
                new(ClaimTypes.Role, roleClaim),
                new("AccountId", accountId.ToString())
            };

            if (branchId.HasValue)
            {
                claims.Add(new Claim("BranchId", branchId.Value.ToString()));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
