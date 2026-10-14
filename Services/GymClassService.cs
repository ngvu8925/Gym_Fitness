using System.Data;
using GymManagement.Api.DTOs;
using GymManagement.Api.Repositories;

namespace GymManagement.Api.Services
{
    public class GymClassService
    {
        private readonly GymClassRepository _repo;

        public GymClassService(GymClassRepository repo) => _repo = repo;
        public GymClassService() : this(new GymClassRepository()) { }

        public DataTable List() => _repo.List();

        public (bool Success, string Message, int ClassId) Add(GymClassRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return (false, "Class name is required", 0);
            if (req.Capacity <= 0)
                return (false, "Capacity must be greater than 0", 0);

            int classId = _repo.Add(req);
            return (true, "Class added successfully", classId);
        }

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
