using System.Security.Claims;

namespace GymManagement.Api.Common
{
    /// <summary>
    /// Helper đọc thông tin từ JWT claims — dùng chung cho mọi controller.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>AccountId đặt trong token (claim NameIdentifier hoặc "AccountId").</summary>
        public static int? GetAccountId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue("AccountId");

            return int.TryParse(value, out var accountId) ? accountId : null;
        }
    }
}
