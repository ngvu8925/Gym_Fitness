using GymManagement.Api.DTOs;
using GymManagement.Api.DTOs.Common;
using GymManagement.Api.Repositories;

namespace GymManagement.Api.Services
{
    public class GymClassService : IGymClassService
    {
        private readonly IGymClassRepository _repo;

        public GymClassService(IGymClassRepository repo) => _repo = repo;

        public PageResult<ClassListItem> List(UserQueryParameters query)
        {
            var dt = _repo.List(query);
            var items = DtoMapper.MapList(dt, row => new ClassListItem
            {
                ClassId = DtoMapper.GetInt(row, "ClassId", "MaLop"),
                BranchId = DtoMapper.GetNullableInt(row, "BranchId", "MaCN"),
                ClassName = DtoMapper.GetString(row, "ClassName", "TenLop") ?? string.Empty,
                TrainerId = DtoMapper.GetNullableInt(row, "TrainerId", "MaHLV"),
                TrainerName = DtoMapper.GetString(row, "TrainerName", "TenHLV"),
                Capacity = DtoMapper.GetInt(row, "Capacity", "SucChua"),
                Status = DtoMapper.GetNullableBool(row, "Status", "TrangThai"),
                DaysOfWeek = DtoMapper.GetString(row, "DaysOfWeek"),
                StartTime = GetTimeSpan(row, "StartTime"),
                EndTime = GetTimeSpan(row, "EndTime")
            });

            // SP đã phân trang ở DB (có cột TotalCount)
            if (dt.Columns.Contains("TotalCount") && dt.Rows.Count > 0)
                return PageResult<ClassListItem>.Of(items, Convert.ToInt32(dt.Rows[0]["TotalCount"]), query);

            // Fallback: SP cũ trả toàn bộ => lọc + cắt tại bộ nhớ
            var search = query.NormalizedKeyword;
            if (search != null)
            {
                items = items.Where(c =>
                    c.ClassName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (c.TrainerName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
            }
            var sorted = query.IsDescending
                ? items.OrderByDescending(c => c.ClassName).ToList()
                : items.OrderBy(c => c.ClassName).ToList();
            return PageResult<ClassListItem>.FromList(sorted, query);
        }

        private static TimeSpan? GetTimeSpan(System.Data.DataRow row, string column)
        {
            var value = DtoMapper.Get(row, column);
            return value == null ? null : (TimeSpan)value;
        }

        public (bool Success, string Message, int ClassId) Add(GymClassRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return (false, "Class name is required", 0);
            if (req.Capacity <= 0)
                return (false, "Capacity must be greater than 0", 0);
            if (req.StartTime.HasValue && req.EndTime.HasValue && req.EndTime.Value <= req.StartTime.Value)
                return (false, "End time must be after start time", 0);

            int classId = _repo.Add(req);
            return (true, "Class added successfully", classId);
        }

        public (bool Success, string Message) Update(int classId, GymClassRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return (false, "Class name is required");
            if (req.Capacity <= 0)
                return (false, "Capacity must be greater than 0");
            if (req.StartTime.HasValue && req.EndTime.HasValue && req.EndTime.Value <= req.StartTime.Value)
                return (false, "End time must be after start time");

            _repo.Update(classId, req);
            return (true, "Class updated successfully");
        }

        public void Delete(int classId) => _repo.Delete(classId);

        public (bool Success, string Message, int EnrollmentId) Enroll(ClassEnrollmentRequest req)
        {
            // Lỗi DB (trùng đăng kí, lớp không tồn tại...) để middleware trả JSON lỗi thống nhất.
            int enrollmentId = _repo.Enroll(req);
            return (true, "Class enrolled successfully", enrollmentId);
        }

        // Trạng thái đăng ký trong DB (giữ khớp với sp_ClassRegistration_Cancel).
        private const string ClassStatusActive = "Đã đăng ký";
        private const string ClassStatusCancelled = "Đã hủy";

        public (bool Success, string Message) Unenroll(int enrollmentId, int memberId)
        {
            var dt = _repo.GetEnrollment(enrollmentId, memberId);
            if (dt.Rows.Count == 0)
                return (false, "Registration not found or does not belong to you");

            var row = dt.Rows[0];
            var status = row["Status"]?.ToString();
            if (status == ClassStatusCancelled)
                return (false, "Registration is already canceled");

            // Quy tắc nghiệp vụ: lớp có giờ bắt đầu và hôm nay là ngày tập
            // -> sau giờ bắt đầu buổi hôm nay thì không cho hủy (lớp đã bắt đầu).
            var daysOfWeek = row.Table.Columns.Contains("DaysOfWeek") ? row["DaysOfWeek"]?.ToString() : null;
            var startTime = row.Table.Columns.Contains("StartTime") && row["StartTime"] != DBNull.Value
                ? (TimeSpan)row["StartTime"]
                : (TimeSpan?)null;

            if (startTime != null && !string.IsNullOrWhiteSpace(daysOfWeek) && IsClassDayToday(daysOfWeek))
            {
                if (DateTime.Now.TimeOfDay >= startTime)
                    return (false, "Class has already started today; cancellation is not allowed");
            }

            int affected = _repo.CancelRegistration(enrollmentId, memberId);
            if (affected == 0)
                return (false, "Registration could not be canceled (already canceled)");

            return (true, "Class registration canceled successfully");
        }

        /// <summary>
        /// DaysOfWeek lưu dạng "2,4,6" (thứ 2, 4, 6); Chủ nhật là 1 hoặc 8.
        /// </summary>
        private static bool IsClassDayToday(string daysOfWeek)
        {
            var today = DateTime.Now.DayOfWeek;               // Mon..Sun
            int todayNum = today == DayOfWeek.Sunday ? 1 : (int)today + 1; // 1=CN, 2=T2..7=T7

            foreach (var part in daysOfWeek.Split(','))
            {
                if (int.TryParse(part.Trim(), out int n))
                {
                    if (n == todayNum) return true;
                    if (todayNum == 1 && (n == 1 || n == 8)) return true; // CN ghi là 1 hoặc 8
                }
            }
            return false;
        }
    }
}
