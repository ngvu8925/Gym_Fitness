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
        private readonly Microsoft.Extensions.Options.IOptions<SecuritySettings>? _security;

        public AccountService(IAccountRepository repo, IMemberRepository memberRepo, IMemberAccountRepository linkRepo,
            Microsoft.Extensions.Options.IOptions<SecuritySettings>? security = null)
        {
            _repo = repo;
            _memberRepo = memberRepo;
            _linkRepo = linkRepo;
            _security = security;
        }

        public (bool Success, string Message, LoginResult? Account) Login(string username, string password, string? ip = null)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return (false, "Please enter both username and password", null);

            var security = _security?.Value ?? new SecuritySettings();
            DataTable dt = _repo.GetByUsername(username);

            // Username không tồn tại: message chung, KHÔNG tiết lộ tài khoản có tồn tại.
            if (dt.Rows.Count == 0)
                return (false, "Invalid username or password", null);

            var row = dt.Rows[0];
            var accountId = Convert.ToInt32(row["AccountId"]);

            // Đang bị khóa tạm -> tự mở khóa nếu đã qua thời hạn.
            var lockoutEnd = row.Table.Columns.Contains("LockoutEnd") && row["LockoutEnd"] != DBNull.Value
                ? (DateTime?)row["LockoutEnd"]
                : null;
            if (lockoutEnd.HasValue && lockoutEnd.Value > DateTime.Now)
            {
                var minutesLeft = (int)Math.Ceiling((lockoutEnd.Value - DateTime.Now).TotalMinutes);
                return (false, $"Account temporarily locked due to failed login attempts. Try again after {minutesLeft} minute(s).", null);
            }

            string storedHash = row["Password"]?.ToString() ?? string.Empty;

            if (!IsPasswordValid(password, storedHash, out var upgradeHash))
            {
                // Ghi nhận lần sai (Task 06); đủ ngưỡng thì khóa tạm.
                var failed = _repo.RecordLoginFailed(accountId, security.MaxFailedAttempts, security.LockoutMinutes);
                var isLocked = failed != null && failed.Rows.Count > 0
                    && failed.Columns.Contains("IsLocked")
                    && Convert.ToInt32(failed.Rows[0]["IsLocked"]) == 1;

                if (failed != null && failed.Rows.Count > 0 && failed.Columns.Contains("FailedLoginCount"))
                {
                    var desc = $"Username='{username}', IP={ip ?? "unknown"}, failedAttempts={failed.Rows[0]["FailedLoginCount"]}";
                    _repo.InsertSystemLog(accountId, isLocked ? "ACCOUNT_LOCKED" : "LOGIN_FAILED", "Accounts", desc);
                }

                if (isLocked)
                    return (false, $"Account temporarily locked due to failed login attempts. Try again after {security.LockoutMinutes} minute(s).", null);

                return (false, "Invalid username or password", null);
            }

            // Tài khoản legacy (SHA-256 cũ) đăng nhập đúng -> nâng cấp lên BCrypt ngay.
            if (upgradeHash != null)
            {
                _repo.UpdatePasswordHash(accountId, upgradeHash);
            }

            // Đăng nhập thành công: reset bộ đếm + xóa khóa + ghi log.
            _repo.RecordLoginSuccess(accountId);
            _repo.InsertSystemLog(accountId, "LOGIN_SUCCESS", "Accounts", $"Username='{username}', IP={ip ?? "unknown"}");

            var result = new LoginResult
            {
                AccountId = accountId,
                FullName = row["FullName"]?.ToString() ?? string.Empty,
                Role = row["Role"]?.ToString() ?? "User",
                BranchId = row.Table.Columns.Contains("BranchId") && row["BranchId"] != DBNull.Value
                    ? Convert.ToInt32(row["BranchId"])
                    : null
            };

            return (true, "Login successful", result);
        }

        public (bool Success, string Message) ChangePassword(int accountId, ChangePasswordRequest req)
        {
            if (req == null
                || string.IsNullOrWhiteSpace(req.CurrentPassword)
                || string.IsNullOrWhiteSpace(req.NewPassword)
                || string.IsNullOrWhiteSpace(req.ConfirmNewPassword))
                return (false, "Current password, new password and confirmation are required");

            // Policy giống khi đăng kí: mật khẩu tối thiểu 6 ký tự.
            if (req.NewPassword.Length < 6)
                return (false, "Password must be at least 6 characters");

            if (req.NewPassword != req.ConfirmNewPassword)
                return (false, "New password and confirmation do not match");

            string? storedHash = _repo.GetPasswordHash(accountId);
            if (storedHash == null)
                return (false, "Account not found");

            // Verify mật khẩu hiện tại (hỗ trợ cả hash legacy như khi đăng nhập).
            if (!IsPasswordValid(req.CurrentPassword, storedHash, out var upgradeHash))
                return (false, "Current password is incorrect");

            // Mật khẩu mới phải khác mật khẩu hiện tại.
            if (upgradeHash == null && PasswordHelper.Verify(req.NewPassword, storedHash))
                return (false, "New password must be different from the current password");

            string newHash = PasswordHelper.Hash(req.NewPassword);
            _repo.UpdatePasswordHash(accountId, newHash);

            return (true, "Password changed successfully");
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
