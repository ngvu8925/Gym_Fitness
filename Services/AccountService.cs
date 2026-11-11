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

            return (true, "Registration successful", accountId);
        }
    }
}
