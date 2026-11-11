using GymManagement.Api.Data;
using GymManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Api.Services
{
    public class MemberAccountService
    {
        private readonly AppDbContext _db;

        public MemberAccountService(AppDbContext db) => _db = db;

        public async Task<Member?> GetMemberForAccountAsync(int accountId)
        {
            var account = await _db.Accounts.SingleOrDefaultAsync(a => a.AccountId == accountId);
            if (account == null || !string.Equals(account.Role, "Member", StringComparison.OrdinalIgnoreCase))
                return null;

            var linked = await _db.Members.SingleOrDefaultAsync(m => m.AccountId == accountId);
            if (linked != null) return linked;

            var matches = await FindUnlinkedMatchesAsync(account);
            if (matches.Count != 1) return null;

            linked = matches[0];
            linked.AccountId = accountId;
            await _db.SaveChangesAsync();
            return linked;
        }

        public async Task<(bool Success, string Message)> LinkAccountAsync(int memberId, int? accountId)
        {
            var member = await _db.Members.SingleOrDefaultAsync(m => m.MemberId == memberId);
            if (member == null) return (false, "Member profile was not found.");

            if (accountId.HasValue)
            {
                var account = await _db.Accounts.SingleOrDefaultAsync(a => a.AccountId == accountId.Value);
                if (account == null || !string.Equals(account.Role, "Member", StringComparison.OrdinalIgnoreCase))
                    return (false, "The selected account does not have the Member role.");

                var alreadyLinked = await _db.Members.AnyAsync(m => m.AccountId == accountId && m.MemberId != memberId);
                if (alreadyLinked) return (false, "This account is already linked to another member.");
            }

            member.AccountId = accountId;
            await _db.SaveChangesAsync();
            return (true, accountId.HasValue ? "Account linked to member." : "Account link removed.");
        }

        private async Task<List<Member>> FindUnlinkedMatchesAsync(Account account)
        {
            var query = _db.Members.Where(m => m.AccountId == null);
            var hasEmail = !string.IsNullOrWhiteSpace(account.Email);
            var hasPhone = !string.IsNullOrWhiteSpace(account.Phone);

            if (hasEmail && hasPhone)
            {
                var email = account.Email!.Trim();
                var phone = account.Phone!.Trim();
                return await query.Where(m => m.Email == email && m.Phone == phone).Take(2).ToListAsync();
            }

            if (hasEmail)
            {
                var email = account.Email!.Trim();
                return await query.Where(m => m.Email == email).Take(2).ToListAsync();
            }

            if (hasPhone)
            {
                var phone = account.Phone!.Trim();
                return await query.Where(m => m.Phone == phone).Take(2).ToListAsync();
            }

            if (!string.IsNullOrWhiteSpace(account.FullName))
            {
                var fullName = account.FullName.Trim();
                return await query.Where(m => m.FullName == fullName).Take(2).ToListAsync();
            }

            return new List<Member>();
        }
    }
}
