using System.Data;
using GymManagement.Api.Common.Exceptions;
using GymManagement.Api.DTOs;
using GymManagement.Api.Models;
using GymManagement.Api.Repositories;

namespace GymManagement.Api.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _repo;
        private readonly IMemberRepository _memberRepo;
        private readonly IMemberAccountRepository _linkRepo;

        public AccountService(IAccountRepository repo, IMemberRepository memberRepo, IMemberAccountRepository linkRepo)
        {
            _repo = repo;
            _memberRepo = memberRepo;
            _linkRepo = linkRepo;
        }

        public (bool Success, string Message, LoginResult? Account) Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return (false, "Please enter both username and password", null);
            DataTable dt = _repo.GetByUsername(username);

            if (dt.Rows.Count == 0)
                return (false, "Invalid username or password", null);

            var row = dt.Rows[0];
            string storedHash = row["Password"]?.ToString() ?? string.Empty;

            if (!IsPasswordValid(password, storedHash, out var upgradeHash))
                return (false, "Invalid username or password", null);

            // Tài khoản legacy (SHA-256 cũ) đăng nhập đúng -> nâng cấp lên BCrypt ngay.
            if (upgradeHash != null)
            {
                _repo.UpdatePasswordHash(Convert.ToInt32(row["AccountId"]), upgradeHash);
            }

            var result = new LoginResult
            {
                AccountId = Convert.ToInt32(row["AccountId"]),
                FullName = row["FullName"]?.ToString() ?? string.Empty,
                Role = row["Role"]?.ToString() ?? "User",
                BranchId = row.Table.Columns.Contains("BranchId") && row["BranchId"] != DBNull.Value
                    ? Convert.ToInt32(row["BranchId"])
                    : null
            };

            return (true, "Login successful", result);
        }

        private bool IsPasswordValid(string password, string storedHash, out string? upgradeHash)
        {
            upgradeHash = null;

            if (PasswordHelper.IsLegacyHash(storedHash))
            {
                if (!PasswordHelper.VerifyLegacy(password, storedHash))
                    return false;

                upgradeHash = PasswordHelper.Hash(password);
                return true;
            }

            return PasswordHelper.Verify(password, storedHash);
        }

        public (bool Success, string Message, int AccountId) Register(
            string username, string password, string fullName, string? phone, string? email)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(fullName))
                return (false, "Username, password and full name are required", 0);

            if (username.Length < 4)
                return (false, "Username must be at least 4 characters", 0);

            if (password.Length < 6)
                return (false, "Password must be at least 6 characters", 0);

            if (_repo.UsernameExists(username))
                return (false, "Username already exists", 0);

            string hash = PasswordHelper.Hash(password);
            int accountId = _repo.Register(username, hash, fullName, phone, email);

            // Tự tạo hồ sơ hội viên + gán chi nhánh mặc định + liên kết với tài khoản,
            // để hội viên đăng nhập là dùng được portal ngay, không cần admin can thiệp.
            int memberId = _memberRepo.Add(new MemberRequest
            {
                FullName = fullName,
                Phone = phone,
                Email = email,
                BranchId = _repo.GetDefaultBranchId()
            });

            int linkResult = _linkRepo.Link(memberId, accountId);
            if (linkResult != 0)
                throw new AppException(
                    "Tài khoản đã tạo nhưng không liên kết được với hồ sơ hội viên. Vui lòng liên hệ quản trị viên.", 500);

            return (true, "Registration successful", accountId);
        }
    }
}
