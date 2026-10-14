using System.Data;
using GymManagement.Api.Repositories;

namespace GymManagement.Api.Services
{
    public class AccountService
    {
        private readonly AccountRepository _repo;

        public AccountService(AccountRepository repo) => _repo = repo;
        public AccountService() : this(new AccountRepository()) { }

        public (bool Success, string Message, DataRow? Account) Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return (false, "Please enter both username and password", null);

            string hash = PasswordHelper.Hash(password);
            DataTable dt = _repo.Login(username, hash);

            if (dt.Rows.Count == 0)
                return (false, "Invalid username or password", null);

            return (true, "Login successful", dt.Rows[0]);
        }
    }
}
