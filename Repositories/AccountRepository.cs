using System.Data;
using Microsoft.Data.SqlClient;

namespace GymManagement.Api.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly DatabaseHelper _db;

        public AccountRepository(DatabaseHelper db) => _db = db;

        /// <summary>
        /// Lấy account theo username (kèm hash mật khẩu) để service tự verify.
        /// BCrypt không so sánh được trong SQL nên không dùng sp_Account_Login nữa.
        /// </summary>
        public DataTable GetByUsername(string username)
        {
            const string sql = @"
                SELECT AccountId, Username, Password, FullName, Role, BranchId, Status
                FROM Accounts
                WHERE Username = @Username;";

            return _db.ExecuteQuery(sql,
                new SqlParameter[] { new SqlParameter("@Username", username) },
                isStoredProcedure: false);
        }

        /// <summary>
        /// Cập nhật lại hash mật khẩu (dùng khi nâng cấp tài khoản legacy sang BCrypt).
        /// </summary>
        public void UpdatePasswordHash(int accountId, string passwordHash)
        {
            const string sql = @"
                UPDATE Accounts SET Password = @Password
                WHERE AccountId = @AccountId;";

            _db.ExecuteNonQuery(sql,
                new SqlParameter[] { new SqlParameter("@AccountId", accountId), new SqlParameter("@Password", passwordHash) },
                isStoredProcedure: false);
        }

        public bool UsernameExists(string username)
        {
            var ps = new SqlParameter[]
            {
                new("@Username", username)
            };
            DataTable dt = _db.ExecuteQuery(
                "SELECT 1 FROM Accounts WHERE Username = @Username",
                ps,
                isStoredProcedure: false);
            return dt.Rows.Count > 0;
        }

        /// <summary>
        /// Chi nhánh mặc định (BranchId nhỏ nhất) để gán cho hồ sơ hội viên tự đăng kí.
        /// </summary>
        public int? GetDefaultBranchId()
        {
            DataTable dt = _db.ExecuteQuery(
                "SELECT TOP 1 BranchId FROM Branches ORDER BY BranchId",
                Array.Empty<SqlParameter>(),
                isStoredProcedure: false);
            return dt.Rows.Count == 0 ? null : Convert.ToInt32(dt.Rows[0][0]);
        }

        public int Register(string username, string passwordHash, string fullName,
            string? phone, string? email)
        {
            var ps = new SqlParameter[]
            {
                new("@Username", username),
                new("@Password", passwordHash),
                new("@FullName", fullName),
                new("@Role", "Member"),
                new("@PhoneNumber", (object?)phone ?? DBNull.Value),
                new("@Email", (object?)email ?? DBNull.Value),
                new("@Status", true),
                new("@CreatedAt", DateTime.UtcNow)
            };

            string sql = @"
                INSERT INTO Accounts (Username, Password, FullName, Role, PhoneNumber, Email, Status, CreatedAt)
                VALUES (@Username, @Password, @FullName, @Role, @PhoneNumber, @Email, @Status, @CreatedAt);
                SELECT SCOPE_IDENTITY();";

            DataTable dt = _db.ExecuteQuery(sql, ps, isStoredProcedure: false);
            return Convert.ToInt32(dt.Rows[0][0]);
        }
    }
}
