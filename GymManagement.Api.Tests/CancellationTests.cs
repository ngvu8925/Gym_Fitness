using System.Data;
using GymManagement.Api.DTOs;
using GymManagement.Api.Repositories;
using GymManagement.Api.Services;
using Moq;

namespace GymManagement.Api.Tests
{
    /// <summary>
    /// Task 03: hủy đăng ký lớp học (Member) + hủy gói tập (Admin).
    /// </summary>
    public class CancellationTests
    {
        // ===== HỦY ĐĂNG KÝ LỚP HỌC (Member) =====

        private static DataTable TaoEnrollment(string status, string? daysOfWeek, object? startTime)
        {
            var dt = new DataTable();
            dt.Columns.Add("ClassRegistrationId", typeof(int));
            dt.Columns.Add("Status", typeof(string));
            dt.Columns.Add("DaysOfWeek", typeof(string));
            dt.Columns.Add("StartTime", typeof(TimeSpan));
            dt.Columns.Add("EndTime", typeof(TimeSpan));
            dt.Rows.Add(5, status, (object?)daysOfWeek ?? DBNull.Value, (object?)startTime ?? DBNull.Value, DBNull.Value);
            return dt;
        }

        private static GymClassService TaoClassService(Mock<IGymClassRepository> repo)
            => new(repo.Object);

        [Fact]
        public void HuyLop_HopLe_CapNhatCuaChinhHoiVien()
        {
            var repo = new Mock<IGymClassRepository>();
            repo.Setup(r => r.GetEnrollment(5, 9)).Returns(TaoEnrollment("Đã đăng ký", null, null));
            repo.Setup(r => r.CancelRegistration(5, 9)).Returns(1);

            var service = TaoClassService(repo);
            var (success, _) = service.Unenroll(5, 9);

            Assert.True(success);
            repo.Verify(r => r.CancelRegistration(5, 9), Times.Once);
        }

        [Fact]
        public void HuyLop_KhongThuocHoiVien_KhongChoHuy()
        {
            var repo = new Mock<IGymClassRepository>();
            // GetEnrollment lọc theo MemberId -> hội viên khác thấy 0 dòng
            repo.Setup(r => r.GetEnrollment(5, 99)).Returns(new DataTable());

            var service = TaoClassService(repo);
            var (success, _) = service.Unenroll(5, 99);

            Assert.False(success);
            repo.Verify(r => r.CancelRegistration(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void HuyLop_DaHuyTruocDo_TuChoiLanNua()
        {
            var repo = new Mock<IGymClassRepository>();
            repo.Setup(r => r.GetEnrollment(5, 9)).Returns(TaoEnrollment("Đã hủy", null, null));

            var service = TaoClassService(repo);
            var (success, _) = service.Unenroll(5, 9);

            Assert.False(success);
            repo.Verify(r => r.CancelRegistration(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void HuyLop_BuoiHomNayDaBatDau_TuChoi()
        {
            var repo = new Mock<IGymClassRepository>();
            // Hôm nay luôn là ngày tập trong test này: DaysOfWeek chứa số thứ tự của hôm nay.
            var today = DateTime.Now.DayOfWeek == DayOfWeek.Sunday ? 1 : (int)DateTime.Now.DayOfWeek + 1;
            repo.Setup(r => r.GetEnrollment(5, 9))
                .Returns(TaoEnrollment("Đã đăng ký", today.ToString(), DateTime.Now.TimeOfDay - TimeSpan.FromMinutes(30)));

            var service = TaoClassService(repo);
            var (success, _) = service.Unenroll(5, 9);

            Assert.False(success);
            repo.Verify(r => r.CancelRegistration(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void HuyLop_BuoiHomNayChuaBatDau_DuocPhep()
        {
            var repo = new Mock<IGymClassRepository>();
            var today = DateTime.Now.DayOfWeek == DayOfWeek.Sunday ? 1 : (int)DateTime.Now.DayOfWeek + 1;
            repo.Setup(r => r.GetEnrollment(5, 9))
                .Returns(TaoEnrollment("Đã đăng ký", today.ToString(), DateTime.Now.TimeOfDay + TimeSpan.FromHours(3)));
            repo.Setup(r => r.CancelRegistration(5, 9)).Returns(1);

            var service = TaoClassService(repo);
            var (success, _) = service.Unenroll(5, 9);

            Assert.True(success);
            repo.Verify(r => r.CancelRegistration(5, 9), Times.Once);
        }

        // ===== HỦY GÓI TẬP (Admin) =====

        private static DataTable TaoPackageReg(string status, DateTime endDate)
        {
            var dt = new DataTable();
            dt.Columns.Add("Status", typeof(string));
            dt.Columns.Add("EndDate", typeof(DateTime));
            dt.Rows.Add(status, endDate);
            return dt;
        }

        private static PackageService TaoPackageService(Mock<IPackageRepository> repo)
            => new(repo.Object);

        [Fact]
        public void HuyGoi_HopLe_CapNhatTrangThaiVaLyDo()
        {
            var repo = new Mock<IPackageRepository>();
            repo.Setup(r => r.GetRegistrationInfo(7)).Returns(TaoPackageReg("Còn hạn", DateTime.Today.AddMonths(3)));
            repo.Setup(r => r.CancelRegistration(7, "Hội viên xin hủy")).Returns(1);

            var service = TaoPackageService(repo);
            var (success, _) = service.CancelRegistration(new CancelPackageRequest { RegistrationId = 7, Reason = "Hội viên xin hủy" });

            Assert.True(success);
            repo.Verify(r => r.CancelRegistration(7, "Hội viên xin hủy"), Times.Once);
        }

        [Fact]
        public void HuyGoi_KhongTonTai_TuChoi()
        {
            var repo = new Mock<IPackageRepository>();
            repo.Setup(r => r.GetRegistrationInfo(999)).Returns(new DataTable());

            var service = TaoPackageService(repo);
            var (success, _) = service.CancelRegistration(new CancelPackageRequest { RegistrationId = 999 });

            Assert.False(success);
            repo.Verify(r => r.CancelRegistration(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void HuyGoi_DaHuyTruocDo_TuChoi()
        {
            var repo = new Mock<IPackageRepository>();
            repo.Setup(r => r.GetRegistrationInfo(7)).Returns(TaoPackageReg("Đã hủy", DateTime.Today.AddMonths(1)));

            var service = TaoPackageService(repo);
            var (success, _) = service.CancelRegistration(new CancelPackageRequest { RegistrationId = 7 });

            Assert.False(success);
            repo.Verify(r => r.CancelRegistration(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void HuyGoi_DaHetHan_TuChoi()
        {
            var repo = new Mock<IPackageRepository>();
            repo.Setup(r => r.GetRegistrationInfo(7)).Returns(TaoPackageReg("Còn hạn", DateTime.Today.AddDays(-1)));

            var service = TaoPackageService(repo);
            var (success, _) = service.CancelRegistration(new CancelPackageRequest { RegistrationId = 7 });

            Assert.False(success);
            repo.Verify(r => r.CancelRegistration(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }
    }
}
