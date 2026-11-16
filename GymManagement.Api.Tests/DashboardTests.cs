using System.Data;
using GymManagement.Api.DTOs;
using GymManagement.Api.Repositories;
using GymManagement.Api.Services;
using Moq;

namespace GymManagement.Api.Tests
{
    /// <summary>
    /// Task 05: endpoint thống kê cho dashboard.
    /// </summary>
    public class DashboardTests
    {
        [Fact]
        public void ActiveRegistrationCount_TraVeSoTuRepo()
        {
            var repo = new Mock<IPackageRepository>();
            repo.Setup(r => r.CountActiveRegistrations()).Returns(5);

            var service = new PackageService(repo.Object);
            var count = service.ActiveRegistrationCount();

            Assert.Equal(5, count);
        }

        [Fact]
        public void ActiveRegistrationCount_KhongCoGoiConHieuLuc_TraVe0()
        {
            var repo = new Mock<IPackageRepository>();
            repo.Setup(r => r.CountActiveRegistrations()).Returns(0);

            var service = new PackageService(repo.Object);

            Assert.Equal(0, service.ActiveRegistrationCount());
        }

        // Regression: chắc chắn nhánh "Package registration not found" không bị mất.
        [Fact]
        public void CancelRegistration_GoiKhongTonTai_VanTuChoi()
        {
            var repo = new Mock<IPackageRepository>();
            repo.Setup(r => r.GetRegistrationInfo(999)).Returns(new DataTable());

            var service = new PackageService(repo.Object);
            var (success, message) = service.CancelRegistration(new CancelPackageRequest { RegistrationId = 999 });

            Assert.False(success);
            Assert.Equal("Package registration not found", message);
        }
    }
}
