using System.Data;
using GymManagement.Api.DTOs;
using GymManagement.Api.Models;
using GymManagement.Api.Repositories;
using GymManagement.Api.Services;
using Microsoft.Extensions.Options;
using Moq;

namespace GymManagement.Api.Tests
{
    /// <summary>
    /// Task 06: giới hạn đăng nhập sai + khóa tài khoản tạm thời.
    /// </summary>
    public class LoginSecurityTests
    {
        private static DataTable TaoAccount(string username, string hash, object? lockoutEnd = null)
        {
            var dt = new DataTable();
            dt.Columns.Add("AccountId", typeof(int));
            dt.Columns.Add("Username", typeof(string));
            dt.Columns.Add("Password", typeof(string));
            dt.Columns.Add("FullName", typeof(string));
            dt.Columns.Add("Role", typeof(string));
            dt.Columns.Add("BranchId", typeof(int));
            dt.Columns.Add("Status", typeof(bool));
            dt.Columns.Add("FailedLoginCount", typeof(int));
            dt.Columns.Add("LockoutEnd", typeof(DateTime));
            dt.Rows.Add(7, username, hash, "Test User", "Member", 1, true, 0, (object?)lockoutEnd ?? DBNull.Value);
            return dt;
        }

        private static DataTable TaoKetQuaSai(int failedCount, bool locked)
        {
            var dt = new DataTable();
            dt.Columns.Add("FailedLoginCount", typeof(int));
            dt.Columns.Add("LockoutEnd", typeof(DateTime));
            dt.Columns.Add("IsLocked", typeof(int));
            dt.Rows.Add(failedCount, locked ? DateTime.Now.AddMinutes(15) : (object)DBNull.Value, locked ? 1 : 0);
            return dt;
        }

        private static AccountService TaoService(Mock<IAccountRepository> repo, SecuritySettings? settings = null)
        {
            var options = Options.Create(settings ?? new SecuritySettings());
            return new AccountService(repo.Object, new Mock<IMemberRepository>().Object,
                new Mock<IMemberAccountRepository>().Object, options);
        }

        [Fact]
        public void SaiDu5Lan_BiKhoaTamThoi()
        {
            var repo = new Mock<IAccountRepository>();
            repo.Setup(r => r.GetByUsername("u1")).Returns(TaoAccount("u1", PasswordHelper.Hash("Dung@123")));
            repo.Setup(r => r.RecordLoginFailed(7, 5, 15)).Returns(TaoKetQuaSai(5, true));

            var service = TaoService(repo, new SecuritySettings { MaxFailedAttempts = 5, LockoutMinutes = 15 });
            var (success, message, _) = service.Login("u1", "Sai@123");

            Assert.False(success);
            Assert.Contains("temporarily locked", message);
            // KHÔNG tiết lộ tài khoản tồn tại trong thông báo khóa trước khi đúng mật khẩu?
            // (username đã đúng nên message khóa là hợp lệ — kiểm tra hành vi chốt ở đây)
            repo.Verify(r => r.InsertSystemLog(7, "ACCOUNT_LOCKED", "Accounts", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void SaiDuoi5Lan_MessageChung_KhongKhoa()
        {
            var repo = new Mock<IAccountRepository>();
            repo.Setup(r => r.GetByUsername("u1")).Returns(TaoAccount("u1", PasswordHelper.Hash("Dung@123")));
            repo.Setup(r => r.RecordLoginFailed(7, 5, 15)).Returns(TaoKetQuaSai(2, false));

            var service = TaoService(repo);
            var (success, message, _) = service.Login("u1", "Sai@123");

            Assert.False(success);
            Assert.Equal("Invalid username or password", message); // không tiết lộ trạng thái khóa
            repo.Verify(r => r.InsertSystemLog(7, "LOGIN_FAILED", "Accounts", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void DangBiKhoa_MatKhauDungCungBiChan()
        {
            var repo = new Mock<IAccountRepository>();
            repo.Setup(r => r.GetByUsername("u1"))
                .Returns(TaoAccount("u1", PasswordHelper.Hash("Dung@123"), DateTime.Now.AddMinutes(10)));

            var service = TaoService(repo);
            var (success, message, _) = service.Login("u1", "Dung@123"); // mật khẩu ĐÚNG nhưng đang khóa

            Assert.False(success);
            Assert.Contains("temporarily locked", message);
            repo.Verify(r => r.RecordLoginSuccess(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void DangNhapThanhCong_ResetCount_GhiLog()
        {
            var repo = new Mock<IAccountRepository>();
            repo.Setup(r => r.GetByUsername("u1")).Returns(TaoAccount("u1", PasswordHelper.Hash("Dung@123")));

            var service = TaoService(repo);
            var (success, _, account) = service.Login("u1", "Dung@123", "127.0.0.1");

            Assert.True(success);
            Assert.NotNull(account);
            repo.Verify(r => r.RecordLoginSuccess(7), Times.Once);
            repo.Verify(r => r.InsertSystemLog(7, "LOGIN_SUCCESS", "Accounts",
                It.Is<string>(d => d.Contains("IP=127.0.0.1") && !d.Contains("Password"))), Times.Once);
        }

        [Fact]
        public void UsernameKhongTonTai_MessageChung_KhongGhiLogTaiKhoan()
        {
            var repo = new Mock<IAccountRepository>();
            repo.Setup(r => r.GetByUsername("ghost")).Returns(new DataTable());

            var service = TaoService(repo);
            var (success, message, _) = service.Login("ghost", "any");

            Assert.False(success);
            Assert.Equal("Invalid username or password", message);
            repo.Verify(r => r.RecordLoginFailed(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            repo.Verify(r => r.InsertSystemLog(It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}
