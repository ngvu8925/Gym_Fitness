using System.Data;
using Microsoft.Data.SqlClient;

namespace GymManagement.Api.Repositories
{
    public class AccountRepository
    {
        private readonly DatabaseHelper _db;

        public AccountRepository(DatabaseHelper db) => _db = db;
        public AccountRepository() : this(new DatabaseHelper()) { }

        public DataTable Login(string username, string passwordHash)
        {
            var ps = new SqlParameter[]
            {
                new("@Username", username),
                new("@PasswordHash", passwordHash)
            };
            return _db.ExecuteQuery("sp_Account_Login", ps);
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
                new("@CreatedAt", DateTime.Now)
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
