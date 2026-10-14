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
    }
}
