using System.Data;
using GymManagement.Api.DTOs;
using GymManagement.Api.Models;
using GymManagement.Api.Repositories;

namespace GymManagement.Api.Services
{
    public class MemberAccountService : IMemberAccountService
    {
        private readonly IMemberAccountRepository _repo;

        public MemberAccountService(IMemberAccountRepository repo) => _repo = repo;

        public Member? GetMemberForAccount(int accountId)
        {
            var account = _repo.GetAccountById(accountId);
            if (account == null || !string.Equals(account["Role"]?.ToString(), "Member", StringComparison.OrdinalIgnoreCase))
                return null;

            var linked = _repo.GetMemberByAccountId(accountId);
            if (linked != null) return linked;

            var matches = _repo.FindUnlinkedMatches(accountId);
            if (matches.Count != 1) return null;

            linked = matches[0];
            _repo.Link(linked.MemberId, accountId);
            linked.AccountId = accountId;
            return linked;
        }

        public DataTable GetMyRegistrations(int memberId) => _repo.GetMyRegistrations(memberId);

        public List<MemberPackageItem> GetMyPackages(int memberId)
        {
            var dt = _repo.GetMyRegistrations(memberId);
            return DtoMapper.MapList(dt, row =>
            {
                decimal listPrice = DtoMapper.GetDecimal(row, "UnitPrice");
                decimal discount = DtoMapper.GetDecimal(row, "DiscountAmount");

                return new MemberPackageItem
                {
                    RegistrationId = DtoMapper.GetInt(row, "RegistrationId"),
                    PackageId = DtoMapper.GetInt(row, "PackageId"),
                    PackageName = DtoMapper.GetString(row, "PackageName") ?? string.Empty,
                    PackageType = DtoMapper.GetString(row, "PackageType") ?? string.Empty,
                    Months = DtoMapper.GetInt(row, "DurationInMonths"),
                    ListPrice = listPrice,
                    DiscountAmount = discount,
                    PaidPrice = listPrice > discount ? listPrice - discount : 0,
                    StartDate = DtoMapper.GetNullableDate(row, "StartDate") ?? default,
                    EndDate = DtoMapper.GetNullableDate(row, "EndDate") ?? default,
                    Status = DtoMapper.GetString(row, "Status") ?? string.Empty
                };
            });
        }

        public List<AccountMemberOptionItem> MemberOptions()
        {
            var dt = _repo.MemberOptions();
            return DtoMapper.MapList(dt, row => new AccountMemberOptionItem
            {
                AccountId = DtoMapper.GetInt(row, "AccountId", "MaTK"),
                Username = DtoMapper.GetString(row, "Username", "TenDangNhap") ?? string.Empty,
                FullName = DtoMapper.GetString(row, "FullName", "HoTen") ?? string.Empty,
                Phone = DtoMapper.GetString(row, "Phone", "PhoneNumber", "SDT"),
                Email = DtoMapper.GetString(row, "Email"),
                MemberId = DtoMapper.GetNullableInt(row, "MemberId", "MaHV")
            });
        }

        public (bool Success, string Message) LinkAccount(int memberId, int? accountId)
        {
            var result = _repo.Link(memberId, accountId);

            return result switch
            {
                0 => (true, accountId.HasValue ? "Account linked to member." : "Account link removed."),
                -1 => (false, "Member profile was not found."),
                -2 => (false, "The selected account does not exist."),
                -3 => (false, "The selected account does not have the Member role."),
                -4 => (false, "This account is already linked to another member."),
                _ => (false, "Unexpected error while linking account.")
            };
        }
    }
}
