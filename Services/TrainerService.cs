using GymManagement.Api.DTOs;
using GymManagement.Api.Repositories;

namespace GymManagement.Api.Services
{
    public class TrainerService : ITrainerService
    {
        private readonly ITrainerRepository _repo;

        public TrainerService(ITrainerRepository repo) => _repo = repo;

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
        public (bool Success, string Message) Update(int trainerId, TrainerRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.FullName))
                return (false, "Full name is required");

            _repo.Update(trainerId, req);
            return (true, "Trainer updated successfully");
        }

        public void Delete(int trainerId) => _repo.Delete(trainerId);
    }
}
