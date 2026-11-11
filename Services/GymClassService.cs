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

            int classId = _repo.Add(req);
            return (true, "Class added successfully", classId);
        }

        public (bool Success, string Message) Update(int classId, GymClassRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return (false, "Class name is required");
            if (req.Capacity <= 0)
                return (false, "Capacity must be greater than 0");

            _repo.Update(classId, req);
            return (true, "Class updated successfully");
        }

        public void Delete(int classId) => _repo.Delete(classId);

        public (bool Success, string Message, int EnrollmentId) Enroll(ClassEnrollmentRequest req)
        {
            try
            {
                int enrollmentId = _repo.Enroll(req);
                return (true, "Class enrolled successfully", enrollmentId);
            }
            catch (Exception ex)
            {
                return (false, "Error: " + ex.Message, 0);
            }
        }
    }
}
