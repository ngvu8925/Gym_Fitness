using System.Data;
using System.Security.Claims;
using System.Security.Principal;
using GymManagement.Api.Common;
using GymManagement.Api.DTOs;
using GymManagement.Api.Repositories;
using GymManagement.Api.Services;
using Moq;

namespace GymManagement.Api.Tests
{
    public class MemberServiceTests
    {
        [Fact]
        public void Add_ThieuHoTen_TraVeFalseVaKhongGoiRepo()
        {
            var repo = new Mock<IMemberRepository>(MockBehavior.Strict);
            var service = new MemberService(repo.Object);

            var (success, message, memberId) = service.Add(new MemberRequest { FullName = "", Phone = "090" });

            Assert.False(success);
            Assert.Equal("Full name is required", message);
            Assert.Equal(0, memberId);
        }

        [Fact]
        public void Add_ThieuSoDienThoai_TraVeFalse()
        {
            var repo = new Mock<IMemberRepository>(MockBehavior.Strict);
            var service = new MemberService(repo.Object);

            var (success, message, _) = service.Add(new MemberRequest { FullName = "Nguyen Van A", Phone = "" });

            Assert.False(success);
            Assert.Equal("Phone number is required", message);
        }

        [Fact]
        public void Add_HopLe_TraVeMemberId()
        {
            var repo = new Mock<IMemberRepository>();
            repo.Setup(r => r.Add(It.IsAny<MemberRequest>())).Returns(7);

            var service = new MemberService(repo.Object);
            var (success, _, memberId) = service.Add(new MemberRequest { FullName = "Nguyen Van A", Phone = "090" });

            Assert.True(success);
            Assert.Equal(7, memberId);
        }

        [Fact]
        public void Update_ThieuHoTen_TraVeFalse()
        {
            var repo = new Mock<IMemberRepository>(MockBehavior.Strict);
            var service = new MemberService(repo.Object);

            var (success, message) = service.Update(1, new MemberRequest { FullName = " " });

            Assert.False(success);
            Assert.Equal("Full name is required", message);
        }
    }

    public class ClaimsPrincipalExtensionsTests
    {
        private static ClaimsPrincipal TaoUser(string nameIdValue)
        {
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, nameIdValue));
            return new ClaimsPrincipal(identity);
        }

        [Fact]
        public void GetAccountId_ClaimSo_ReturnGiaTriDung()
        {
            var user = TaoUser("42");
            Assert.Equal(42, user.GetAccountId());
        }

        [Fact]
        public void GetAccountId_ClaimKhongPhaiSo_ReturnNull()
        {
            var user = TaoUser("khong-phai-so");
            Assert.Null(user.GetAccountId());
        }
    }
}
