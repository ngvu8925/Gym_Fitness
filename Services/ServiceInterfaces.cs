using GymManagement.Api.DTOs;
using GymManagement.Api.DTOs.Common;
using GymManagement.Api.Models;
using GymManagement.Api.Repositories;

namespace GymManagement.Api.Services
{
    public interface IMemberService
    {
        (bool Success, string Message, int MemberId) Add(MemberRequest member);
        (bool Success, string Message) Update(int memberId, MemberRequest member);
        void Delete(int memberId);
        MemberListItem? GetById(int memberId);
        PageResult<MemberListItem> Search(string? keyword, UserQueryParameters query);
    }

    public interface IPackageService
    {
        PageResult<PackageListItem> List(UserQueryParameters query);
        (bool Success, string Message) Add(PackageRequest req);
        (bool Success, string Message) Update(int packageId, PackageRequest req);
        void Delete(int packageId);
        (bool Success, string Message, int RegistrationId, decimal DiscountAmount) Register(PackageRegistrationRequest req);
        void Pause(PausePackageRequest req);
        void Resume(int pauseId);
        List<ExpiringMemberItem> ExpiringSoon(int days);
    }

    public interface IGymClassService
    {
        PageResult<ClassListItem> List(UserQueryParameters query);
        (bool Success, string Message, int ClassId) Add(GymClassRequest req);
        (bool Success, string Message) Update(int classId, GymClassRequest req);
        void Delete(int classId);
        (bool Success, string Message, int EnrollmentId) Enroll(ClassEnrollmentRequest req);
    }

    public interface ITrainerService
    {
        PageResult<TrainerListItem> List(UserQueryParameters query);
        (bool Success, string Message, int TrainerId) Add(TrainerRequest req);
        (bool Success, string Message) Update(int trainerId, TrainerRequest req);
        void Delete(int trainerId);
    }

    public interface IAttendanceService
    {
        (bool Success, string Message) CheckIn(int memberId);
        void CheckOut(int attendanceId);
    }

    public interface IReportService
    {
        List<ReportItem> MonthlyRevenue(int year, int month);
        List<ReportItem> AllMonthlyRevenue();
        List<ReportItem> BestSellingPackages();
        List<ReportItem> BranchRevenue();
    }

    public interface IAccountService
    {
        (bool Success, string Message, LoginResult? Account) Login(string username, string password);
        (bool Success, string Message, int AccountId) Register(string username, string password, string fullName, string? phone, string? email);
        (bool Success, string Message) ChangePassword(int accountId, ChangePasswordRequest req);
    }

    public interface IMemberAccountService
    {
        Member? GetMemberForAccount(int accountId);
        List<MemberPackageItem> GetMyPackages(int memberId);
        List<MemberClassItem> GetMyClasses(int memberId);
        List<MemberAttendanceItem> GetMyAttendance(int memberId, int top = 10);
        List<AccountMemberOptionItem> MemberOptions();
        (bool Success, string Message) LinkAccount(int memberId, int? accountId);
    }
}
