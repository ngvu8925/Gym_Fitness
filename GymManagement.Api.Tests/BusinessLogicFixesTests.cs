using System.Data;
using System.Security.Cryptography;
using System.Text;
using GymManagement.Api.Common.Exceptions;
using GymManagement.Api.DTOs;
using GymManagement.Api.Repositories;
using GymManagement.Api.Services;
using Moq;

namespace GymManagement.Api.Tests
{
    public class BusinessLogicFixesTests
    {
        [Fact]
        public void Login_TaiKhoanBiVoHieuHoa_BiChan()
        {
            var dt = new DataTable();
            dt.Columns.Add("AccountId", typeof(int));
            dt.Columns.Add("Username", typeof(string));
            dt.Columns.Add("Password", typeof(string));
            dt.Columns.Add("FullName", typeof(string));
            dt.Columns.Add("Role", typeof(string));
            dt.Columns.Add("BranchId", typeof(int));
            dt.Columns.Add("Status", typeof(bool));
            dt.Rows.Add(10, "disabled_user", PasswordHelper.Hash("Dung@123"), "Disabled User", "Member", 1, false);

            var repo = new Mock<IAccountRepository>();
            repo.Setup(r => r.GetByUsername("disabled_user")).Returns(dt);

            var service = new AccountService(repo.Object, new Mock<IMemberRepository>().Object, new Mock<IMemberAccountRepository>().Object);
            var (success, message, account) = service.Login("disabled_user", "Dung@123");

            Assert.False(success);
            Assert.Null(account);
            Assert.Contains("deactivated", message, StringComparison.OrdinalIgnoreCase);
            repo.Verify(r => r.RecordLoginSuccess(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void DoiMatKhau_TaiKhoanLegacyTrungMatKhauCu_BiChan()
        {
            var legacyHash = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes("Legacy@123"))).ToLowerInvariant();

            var repo = new Mock<IAccountRepository>();
            repo.Setup(r => r.GetPasswordHash(1)).Returns(legacyHash);

            var service = new AccountService(repo.Object, new Mock<IMemberRepository>().Object, new Mock<IMemberAccountRepository>().Object);
            var (success, message) = service.ChangePassword(1, new ChangePasswordRequest
            {
                CurrentPassword = "Legacy@123",
                NewPassword = "Legacy@123",
                ConfirmNewPassword = "Legacy@123"
            });

            Assert.False(success);
            Assert.Contains("different", message, StringComparison.OrdinalIgnoreCase);
            repo.Verify(r => r.UpdatePasswordHash(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Register_TaoMemberThatBai_RollbackXoaAccount()
        {
            var repo = new Mock<IAccountRepository>();
            repo.Setup(r => r.UsernameExists("newuser")).Returns(false);
            repo.Setup(r => r.Register("newuser", It.IsAny<string>(), "Nguyen Van B", "0901234567", null)).Returns(42);
            repo.Setup(r => r.GetDefaultBranchId()).Returns(1);

            var memberRepo = new Mock<IMemberRepository>();
            memberRepo.Setup(r => r.Add(It.IsAny<MemberRequest>())).Throws(new InvalidOperationException("Duplicate phone"));

            var service = new AccountService(repo.Object, memberRepo.Object, new Mock<IMemberAccountRepository>().Object);

            Assert.Throws<InvalidOperationException>(() => service.Register("newuser", "123456", "Nguyen Van B", "0901234567", null));
            repo.Verify(r => r.DeleteAccount(42), Times.Once);
        }

        [Fact]
        public void Register_LinkThatBai_RollbackXoaMemberVaAccount()
        {
            var repo = new Mock<IAccountRepository>();
            repo.Setup(r => r.UsernameExists("newuser")).Returns(false);
            repo.Setup(r => r.Register("newuser", It.IsAny<string>(), "Nguyen Van B", null, null)).Returns(42);
            repo.Setup(r => r.GetDefaultBranchId()).Returns(1);

            var memberRepo = new Mock<IMemberRepository>();
            memberRepo.Setup(r => r.Add(It.IsAny<MemberRequest>())).Returns(99);

            var linkRepo = new Mock<IMemberAccountRepository>();
            linkRepo.Setup(r => r.Link(99, 42)).Returns(-4);

            var service = new AccountService(repo.Object, memberRepo.Object, linkRepo.Object);

            Assert.Throws<AppException>(() => service.Register("newuser", "123456", "Nguyen Van B", null, null));
            memberRepo.Verify(r => r.Delete(99), Times.Once);
            repo.Verify(r => r.DeleteAccount(42), Times.Once);
        }

        [Fact]
        public void ClassAdd_GioKetThucTruocGioBatDau_TraVeFalse()
        {
            var repo = new Mock<IGymClassRepository>(MockBehavior.Strict);
            var service = new GymClassService(repo.Object);

            var (success, message, classId) = service.Add(new GymClassRequest
            {
                Name = "Yoga Sáng",
                Capacity = 15,
                StartTime = new TimeSpan(19, 0, 0),
                EndTime = new TimeSpan(18, 0, 0)
            });

            Assert.False(success);
            Assert.Equal(0, classId);
            Assert.Contains("after start time", message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ClassUpdate_GioKetThucBangGioBatDau_TraVeFalse()
        {
            var repo = new Mock<IGymClassRepository>(MockBehavior.Strict);
            var service = new GymClassService(repo.Object);

            var (success, message) = service.Update(1, new GymClassRequest
            {
                Name = "Yoga Sáng",
                Capacity = 15,
                StartTime = new TimeSpan(18, 0, 0),
                EndTime = new TimeSpan(18, 0, 0)
            });

            Assert.False(success);
            Assert.Contains("after start time", message, StringComparison.OrdinalIgnoreCase);
        }
    }
}
