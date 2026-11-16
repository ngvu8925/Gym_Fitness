using System.Data;
using GymManagement.Api.Common.Exceptions;
using GymManagement.Api.DTOs;
using GymManagement.Api.Repositories;
using GymManagement.Api.Services;
using Moq;

namespace GymManagement.Api.Tests
{
    public class AccountServiceTests
    {
        private static DataTable TaoAccountTable(string hash)
        {
            var dt = new DataTable();
            dt.Columns.Add("AccountId", typeof(int));
            dt.Columns.Add("Username", typeof(string));
            dt.Columns.Add("Password", typeof(string));
            dt.Columns.Add("FullName", typeof(string));
            dt.Columns.Add("Role", typeof(string));
            dt.Columns.Add("BranchId", typeof(int));
            dt.Rows.Add(1, "testuser", hash, "Test User", "Member", 1);
            return dt;
        }

        [Fact]
        public void Login_ThieuUsernameHoacPassword_TraVeFalse()
        {
            var repo = new Mock<IAccountRepository>(MockBehavior.Strict);
            var service = new AccountService(repo.Object, new Mock<IMemberRepository>().Object, new Mock<IMemberAccountRepository>().Object);

            var (success, _, _) = service.Login("", "pass");
            Assert.False(success);

            (success, _, _) = service.Login("user", "");
            Assert.False(success);
        }

        [Fact]
        public void Login_SaiMatKhau_TraVeFalse()
        {
            var repo = new Mock<IAccountRepository>();
            var bcryptHash = PasswordHelper.Hash("Dung@123");
            repo.Setup(r => r.GetByUsername("testuser")).Returns(TaoAccountTable(bcryptHash));

            var service = new AccountService(repo.Object, new Mock<IMemberRepository>().Object, new Mock<IMemberAccountRepository>().Object);
            var (success, _, account) = service.Login("testuser", "Sai@123");

            Assert.False(success);
            Assert.Null(account);
        }

        [Fact]
        public void Login_DungMatKhauBcrypt_TraVeLoginResult()
        {
            var repo = new Mock<IAccountRepository>();
            repo.Setup(r => r.GetByUsername("testuser")).Returns(TaoAccountTable(PasswordHelper.Hash("Dung@123")));

            var service = new AccountService(repo.Object, new Mock<IMemberRepository>().Object, new Mock<IMemberAccountRepository>().Object);
            var (success, _, account) = service.Login("testuser", "Dung@123");

            Assert.True(success);
            Assert.NotNull(account);
            Assert.Equal(1, account!.AccountId);
            Assert.Equal("Test User", account.FullName);
            Assert.Equal("Member", account.Role);
            Assert.Equal(1, account.BranchId);
        }

        [Fact]
        public void Login_TaiKhoanLegacyDangNhapDung_TuDongNangCapBcrypt()
        {
            var legacyHash = Convert.ToHexString(
                System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("Legacy@123"))).ToLowerInvariant();

            var repo = new Mock<IAccountRepository>();
            repo.Setup(r => r.GetByUsername("testuser")).Returns(TaoAccountTable(legacyHash));

            var service = new AccountService(repo.Object, new Mock<IMemberRepository>().Object, new Mock<IMemberAccountRepository>().Object);
            var (success, _, account) = service.Login("testuser", "Legacy@123");

            Assert.True(success);
            Assert.NotNull(account);
            repo.Verify(r => r.UpdatePasswordHash(1, It.Is<string>(h => h.StartsWith("$2"))), Times.Once);
        }

        [Fact]
        public void Register_UsernameQuaNgon_TraVeFalse()
        {
            var repo = new Mock<IAccountRepository>(MockBehavior.Strict);
            var service = new AccountService(repo.Object, new Mock<IMemberRepository>().Object, new Mock<IMemberAccountRepository>().Object);

            var (success, _, _) = service.Register("abc", "123456", "Nguyen Van A", null, null);

            Assert.False(success);
        }

        [Fact]
        public void Register_PasswordQuaNgon_TraVeFalse()
        {
            var repo = new Mock<IAccountRepository>(MockBehavior.Strict);
            var service = new AccountService(repo.Object, new Mock<IMemberRepository>().Object, new Mock<IMemberAccountRepository>().Object);

            var (success, _, _) = service.Register("nguoidung", "123", "Nguyen Van A", null, null);

            Assert.False(success);
        }

        [Fact]
        public void Register_UsernameDaTonTai_TraVeFalse()
        {
            var repo = new Mock<IAccountRepository>();
            repo.Setup(r => r.UsernameExists("nguoidung")).Returns(true);

            var service = new AccountService(repo.Object, new Mock<IMemberRepository>().Object, new Mock<IMemberAccountRepository>().Object);
            var (success, _, _) = service.Register("nguoidung", "123456", "Nguyen Van A", null, null);

            Assert.False(success);
            repo.Verify(r => r.Register(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
        }

        [Fact]
        public void Register_HopLe_GoiRepoVaTraVeAccountId()
        {
            var repo = new Mock<IAccountRepository>();
            repo.Setup(r => r.UsernameExists("nguoidung")).Returns(false);
            repo.Setup(r => r.Register("nguoidung", It.IsAny<string>(), "Nguyen Van A", "0900000000", "a@test.com")).Returns(9);

            var service = new AccountService(repo.Object, new Mock<IMemberRepository>().Object, new Mock<IMemberAccountRepository>().Object);
            var (success, _, accountId) = service.Register("nguoidung", "123456", "Nguyen Van A", "0900000000", "a@test.com");

            Assert.True(success);
            Assert.Equal(9, accountId);
            repo.Verify(r => r.Register("nguoidung", It.Is<string>(h => h.StartsWith("$2")), "Nguyen Van A", "0900000000", "a@test.com"), Times.Once);
        }

        [Fact]
        public void Register_HopLe_TuTaoHoSoHoiVienVaLienKet()
        {
            var repo = new Mock<IAccountRepository>();
            repo.Setup(r => r.UsernameExists("nguoidung")).Returns(false);
            repo.Setup(r => r.Register(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>())).Returns(9);
            repo.Setup(r => r.GetDefaultBranchId()).Returns(1);

            var memberRepo = new Mock<IMemberRepository>();
            memberRepo.Setup(r => r.Add(It.Is<MemberRequest>(m => m.FullName == "Nguyen Van A" && m.BranchId == 1))).Returns(55);

            var linkRepo = new Mock<IMemberAccountRepository>();

            var service = new AccountService(repo.Object, memberRepo.Object, linkRepo.Object);
            var (success, _, accountId) = service.Register("nguoidung", "123456", "Nguyen Van A", null, null);

            Assert.True(success);
            Assert.Equal(9, accountId);
            memberRepo.Verify(r => r.Add(It.Is<MemberRequest>(m => m.FullName == "Nguyen Van A" && m.BranchId == 1)), Times.Once);
            linkRepo.Verify(r => r.Link(55, 9), Times.Once);
        }

        [Fact]
        public void Register_LienKetThatBai_TraVeFalse()
        {
            var repo = new Mock<IAccountRepository>();
            repo.Setup(r => r.UsernameExists("nguoidung")).Returns(false);
            repo.Setup(r => r.Register(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>())).Returns(9);
            repo.Setup(r => r.GetDefaultBranchId()).Returns((int?)1);

            var memberRepo = new Mock<IMemberRepository>();
            memberRepo.Setup(r => r.Add(It.IsAny<MemberRequest>())).Returns(55);

            var linkRepo = new Mock<IMemberAccountRepository>();
            linkRepo.Setup(r => r.Link(55, 9)).Returns(-4);

            var service = new AccountService(repo.Object, memberRepo.Object, linkRepo.Object);

            Assert.Throws<AppException>(() => service.Register("nguoidung", "123456", "Nguyen Van A", null, null));
        }
    }
}

