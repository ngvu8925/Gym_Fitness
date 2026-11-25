using System.Data;
using GymManagement.Api.DTOs;
using GymManagement.Api.DTOs.Common;
using GymManagement.Api.Models;

namespace GymManagement.Api.Repositories
{
    public interface IMemberRepository
    {
        int Add(MemberRequest member);
        void Update(int memberId, MemberRequest member);
        void Delete(int memberId);
        DataTable Search(string? keyword, UserQueryParameters query);
        DataTable GetById(int memberId);
    }

    public interface IPackageRepository
    {
        DataTable List(UserQueryParameters query);
        void Add(PackageRequest req);
        void Update(int packageId, PackageRequest req);
        void Delete(int packageId);
        (int RegistrationId, decimal DiscountAmount) Register(PackageRegistrationRequest req);
        void Pause(PausePackageRequest req);
        void Resume(int pauseId);
        DataTable GetRegistrationInfo(int registrationId);
        int CancelRegistration(int registrationId, string reason);
        int CountActiveRegistrations();
        DataTable ExpiringSoon(int days);
    }

    public interface IGymClassRepository
    {
        DataTable List(UserQueryParameters query);
        int Add(GymClassRequest req);
        void Update(int classId, GymClassRequest req);
        void Delete(int classId);
        int Enroll(ClassEnrollmentRequest req);
        DataTable GetEnrollment(int enrollmentId, int memberId);
        int CancelRegistration(int enrollmentId, int memberId);
    }

    public interface ITrainerRepository
    {
        DataTable List(UserQueryParameters query);
        int Add(TrainerRequest req);
        void Update(int trainerId, TrainerRequest req);
        void Delete(int trainerId);
    }

    public interface IAttendanceRepository
    {
        void CheckIn(int memberId);
        void CheckOut(int attendanceId);
    }

    public interface IReportRepository
    {
        DataTable MonthlyRevenue(int year, int month);
        DataTable AllMonthlyRevenue();
        DataTable BestSellingPackages();
        DataTable BranchRevenue();
    }

    public interface IAccountRepository
    {
        DataTable GetByUsername(string username);
        void UpdatePasswordHash(int accountId, string passwordHash);
        bool UsernameExists(string username);
        int Register(string username, string passwordHash, string fullName, string? phone, string? email);
        int? GetDefaultBranchId();
        string? GetPasswordHash(int accountId);
        DataTable RecordLoginFailed(int accountId, int maxFailedAttempts, int lockoutMinutes);
        void RecordLoginSuccess(int accountId);
        void InsertSystemLog(int? accountId, string action, string? tableName, string? description);
        void DeleteAccount(int accountId);
    }

    public interface IMemberAccountRepository
    {
        DataRow? GetAccountById(int accountId);
        Member? GetMemberByAccountId(int accountId);
        List<Member> FindUnlinkedMatches(int accountId);
        int Link(int memberId, int? accountId);
        DataTable MemberOptions();
        DataTable GetMyRegistrations(int memberId);
        DataTable GetMyClasses(int memberId);
        DataTable GetMyAttendance(int memberId, int top = 10);
    }
}
