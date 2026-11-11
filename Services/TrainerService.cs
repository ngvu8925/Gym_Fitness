using GymManagement.Api.DTOs;
using GymManagement.Api.DTOs.Common;
using GymManagement.Api.Repositories;

namespace GymManagement.Api.Services
{
    public class TrainerService : ITrainerService
    {
        private readonly ITrainerRepository _repo;

        public TrainerService(ITrainerRepository repo) => _repo = repo;

        public PageResult<TrainerListItem> List(UserQueryParameters query)
        {
            var dt = _repo.List(query);
            var items = DtoMapper.MapList(dt, row => new TrainerListItem
            {
                TrainerId = DtoMapper.GetInt(row, "TrainerId", "MaHLV"),
                BranchId = DtoMapper.GetNullableInt(row, "BranchId", "MaCN"),
                FullName = DtoMapper.GetString(row, "FullName", "HoTen", "TrainerName", "TenHLV") ?? string.Empty,
                Specialty = DtoMapper.GetString(row, "Specialty", "ChuyenMon"),
                Phone = DtoMapper.GetString(row, "PhoneNumber", "Phone", "SDT"),
                HireDate = DtoMapper.GetNullableDate(row, "HireDate", "NgayVaoLam")
            });

            // SP đã phân trang ở DB (có cột TotalCount)
            if (dt.Columns.Contains("TotalCount") && dt.Rows.Count > 0)
                return PageResult<TrainerListItem>.Of(items, Convert.ToInt32(dt.Rows[0]["TotalCount"]), query);

            // Fallback: SP cũ trả toàn bộ => lọc + cắt tại bộ nhớ
            var search = query.NormalizedKeyword;
            if (search != null)
            {
                items = items.Where(t =>
                    t.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (t.Specialty?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
            }
            var sorted = query.IsDescending
                ? items.OrderByDescending(t => t.FullName).ToList()
                : items.OrderBy(t => t.FullName).ToList();
            return PageResult<TrainerListItem>.FromList(sorted, query);
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
