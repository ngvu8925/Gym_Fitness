using System.Data;
using GymManagement.Api.DTOs;
using Microsoft.Data.SqlClient;

namespace GymManagement.Api.Repositories
{
    public class PackageRepository
    {
        private readonly DatabaseHelper _db;

        public PackageRepository(DatabaseHelper db) => _db = db;
        public PackageRepository() : this(new DatabaseHelper()) { }

        public DataTable List() => _db.ExecuteQuery("sp_MembershipPackage_GetAll");

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

        public (int RegistrationId, decimal DiscountAmount) Register(PackageRegistrationRequest req)
        {
            var paymentMethod = string.Equals(req.PaymentMethod, "Cash", StringComparison.OrdinalIgnoreCase)
                ? "Tiền mặt"
                : req.PaymentMethod;

            var ps = new SqlParameter[]
            {
                new("@MemberId", req.MemberId),
                new("@PackageId", req.PackageId),
                new("@BranchId", req.BranchId),
                new("@StartDate", req.StartDate),
                new("@AccountId", req.AccountId),
                new("@PromoCode", (object?)req.PromoCode ?? DBNull.Value),
                new("@PaymentMethod", paymentMethod)
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

        public DataTable ExpiringSoon(int days)
        {
            return _db.ExecuteQuery("sp_Member_GetExpiringSoon", new SqlParameter("@Days", days));
        }
    }
}
