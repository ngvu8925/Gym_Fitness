using GymManagement.Api.DTOs;
using GymManagement.Api.Repositories;

namespace GymManagement.Api.Services
{
    public class GymClassService
    {
        private readonly GymClassRepository _repo;

        public GymClassService(GymClassRepository repo) => _repo = repo;

        public List<ClassListItem> List()
        {
            var dt = _repo.List();
            return DtoMapper.MapList(dt, row => new ClassListItem
            {
                ClassId = DtoMapper.GetInt(row, "ClassId", "MaLop"),
                BranchId = DtoMapper.GetNullableInt(row, "BranchId", "MaCN"),
                ClassName = DtoMapper.GetString(row, "ClassName", "TenLop") ?? string.Empty,
                TrainerId = DtoMapper.GetNullableInt(row, "TrainerId", "MaHLV"),
                TrainerName = DtoMapper.GetString(row, "TrainerName", "TenHLV"),
                Capacity = DtoMapper.GetInt(row, "Capacity", "SucChua"),
                Status = DtoMapper.GetNullableBool(row, "Status", "TrangThai")
            });
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
