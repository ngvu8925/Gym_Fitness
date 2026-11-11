using GymManagement.Api.DTOs;
using GymManagement.Api.Repositories;

namespace GymManagement.Api.Services
{
    public class TrainerService
    {
        private readonly TrainerRepository _repo;

        public TrainerService(TrainerRepository repo) => _repo = repo;

        public List<TrainerListItem> List()
        {
            var dt = _repo.List();
            return DtoMapper.MapList(dt, row => new TrainerListItem
            {
                TrainerId = DtoMapper.GetInt(row, "TrainerId", "MaHLV"),
                BranchId = DtoMapper.GetNullableInt(row, "BranchId", "MaCN"),
                FullName = DtoMapper.GetString(row, "FullName", "HoTen", "TrainerName", "TenHLV") ?? string.Empty,
                Specialty = DtoMapper.GetString(row, "Specialty", "ChuyenMon"),
                Phone = DtoMapper.GetString(row, "PhoneNumber", "Phone", "SDT"),
                HireDate = DtoMapper.GetNullableDate(row, "HireDate", "NgayVaoLam")
            });
        }

        public (bool Success, string Message, int TrainerId) Add(TrainerRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.FullName))
                return (false, "Full name is required", 0);

            int trainerId = _repo.Add(req);
            return (true, "Trainer added successfully", trainerId);
        }
    }
}
