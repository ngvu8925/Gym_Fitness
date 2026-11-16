using System.Data;
using GymManagement.Api.DTOs;
using Microsoft.Data.SqlClient;

namespace GymManagement.Api.Repositories
{
    public class PackageRepository : IPackageRepository
    {
        private readonly DatabaseHelper _db;

        public PackageRepository(DatabaseHelper db) => _db = db;

        public DataTable List(DTOs.Common.UserQueryParameters query)
        {
            var ps = new SqlParameter[]
            {
                new("@Keyword", query.NormalizedKeyword ?? (object)DBNull.Value),
                new("@Page", query.Page),
                new("@PageSize", query.PageSize),
                new("@SortDir", query.IsDescending ? "DESC" : "ASC")
            };
            return PagedQuery.Execute(_db, "sp_MembershipPackage_GetAll", ps,
                legacyCall: () => _db.ExecuteQuery("sp_MembershipPackage_GetAll"));
        }

        public void Add(PackageRequest req)
        {
            var ps = new SqlParameter[]
            {
                new("@PackageName", req.Name),
                new("@DurationInMonths", req.Months),
                new("@UnitPrice", req.Price),
                new("@Description", (object?)req.Description ?? DBNull.Value)
            };
            _db.ExecuteNonQuery("sp_MembershipPackage_Create", ps);
        }

        public void Update(int packageId, PackageRequest req)
        {
            var ps = new SqlParameter[]
            {
                new("@PackageId", packageId),
                new("@PackageName", req.Name),
                new("@PackageType", req.Type),
                new("@DurationInMonths", req.Months),
                new("@UnitPrice", req.Price),
                new("@Description", (object?)req.Description ?? DBNull.Value)
            };
            _db.ExecuteNonQuery("sp_MembershipPackage_Update", ps);
        }

        public void Delete(int packageId)
        {
            _db.ExecuteNonQuery("sp_MembershipPackage_Delete", new SqlParameter("@PackageId", packageId));
        }

        public (int RegistrationId, decimal DiscountAmount) Register(PackageRegistrationRequest req)
        {
            var ps = new SqlParameter[]
            {
                new("@MemberId", req.MemberId),
                new("@PackageId", req.PackageId),
                new("@BranchId", req.BranchId),
                new("@StartDate", req.StartDate),
                new("@AccountId", req.AccountId),
                new("@PromoCode", (object?)req.PromoCode ?? DBNull.Value),
                new("@PaymentMethod", req.PaymentMethod)
            };
            var dt = _db.ExecuteQuery("sp_PackageRegistration_Create", ps);
            int registrationId = Convert.ToInt32(dt.Rows[0]["RegistrationId"]);
            decimal discountAmount = Convert.ToDecimal(dt.Rows[0]["DiscountAmount"]);
            return (registrationId, discountAmount);
        }

        public void Pause(PausePackageRequest req)
        {
            var ps = new SqlParameter[]
            {
                new("@RegistrationId", req.RegistrationId),
                new("@Reason", req.Reason)
            };
            _db.ExecuteNonQuery("sp_PackageRegistration_Suspend", ps);
        }

        public void Resume(int pauseId)
        {
            _db.ExecuteNonQuery("sp_PackageRegistration_Resume", new SqlParameter("@SuspensionId", pauseId));
        }

        /// <summary>
        /// Trạng thái + ngày kết thúc của 1 đăng ký gói (dùng kiểm tra trước khi hủy).
        /// </summary>
        public DataTable GetRegistrationInfo(int registrationId)
        {
            return _db.ExecuteQuery(
                "SELECT Status, EndDate FROM dbo.PackageRegistrations WHERE RegistrationId = @RegistrationId",
                new SqlParameter[] { new("@RegistrationId", registrationId) },
                isStoredProcedure: false);
        }

        /// <summary>
        /// Admin hủy gói — SP chỉ cập nhật gói còn hạn/tạm ngưng, trả về số dòng ảnh hưởng.
        /// Không xóa Payments (lịch sử thanh toán bảo toàn).
        /// </summary>
        public int CancelRegistration(int registrationId, string reason)
        {
            var dt = _db.ExecuteQuery("sp_PackageRegistration_Cancel", new SqlParameter[]
            {
                new("@RegistrationId", registrationId),
                new("@Reason", (object?)reason ?? DBNull.Value)
            });
            return Convert.ToInt32(dt.Rows[0]["AffectedRows"]);
        }

        public DataTable ExpiringSoon(int days)
        {
            return _db.ExecuteQuery("sp_Member_GetExpiringSoon", new SqlParameter("@Days", days));
        }
    }
}
