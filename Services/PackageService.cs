using GymManagement.Api.DTOs;
using GymManagement.Api.DTOs.Common;
using GymManagement.Api.Repositories;

namespace GymManagement.Api.Services
{
    public class PackageService : IPackageService
    {
        private readonly IPackageRepository _repo;

        public PackageService(IPackageRepository repo) => _repo = repo;

        public PageResult<PackageListItem> List(UserQueryParameters query)
        {
            var dt = _repo.List(query);
            var items = DtoMapper.MapList(dt, row => new PackageListItem
            {
                PackageId = DtoMapper.GetInt(row, "PackageId", "MaGoi"),
                PackageName = DtoMapper.GetString(row, "PackageName", "TenGoi") ?? string.Empty,
                PackageType = DtoMapper.GetString(row, "PackageType", "LoaiGoi"),
                Months = DtoMapper.GetInt(row, "DurationInMonths", "SoThang"),
                Price = DtoMapper.GetDecimal(row, "UnitPrice", "DonGia"),
                Description = DtoMapper.GetString(row, "Description", "MoTa"),
                Status = DtoMapper.GetNullableBool(row, "Status", "TrangThai")
            });

            // SP đã phân trang ở DB (có cột TotalCount)
            if (dt.Columns.Contains("TotalCount") && dt.Rows.Count > 0)
                return PageResult<PackageListItem>.Of(items, Convert.ToInt32(dt.Rows[0]["TotalCount"]), query);

            // Fallback: SP cũ trả toàn bộ => lọc + cắt tại bộ nhớ
            var search = query.NormalizedKeyword;
            if (search != null)
            {
                items = items.Where(p =>
                    p.PackageName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (p.PackageType?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
            }
            var sorted = query.IsDescending
                ? items.OrderByDescending(p => p.PackageName).ToList()
                : items.OrderBy(p => p.PackageName).ToList();
            return PageResult<PackageListItem>.FromList(sorted, query);
        }

        public (bool Success, string Message) Add(PackageRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return (false, "Package name is required");
            if (req.Months <= 0)
                return (false, "Months must be greater than 0");
            if (req.Price < 0)
                return (false, "Price is invalid");

            _repo.Add(req);
            return (true, "Package added successfully");
        }

        public (bool Success, string Message) Update(int packageId, PackageRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return (false, "Package name is required");
            if (req.Months <= 0)
                return (false, "Months must be greater than 0");
            if (req.Price < 0)
                return (false, "Price is invalid");

            _repo.Update(packageId, req);
            return (true, "Package updated successfully");
        }

        public void Delete(int packageId) => _repo.Delete(packageId);

        public (bool Success, string Message, int RegistrationId, decimal DiscountAmount) Register(PackageRegistrationRequest req)
        {
            if (req.StartDate.Date < DateTime.Today)
                return (false, "Start date cannot be in the past", 0, 0);

            try
            {
                // Business rule: hiển thị "Cash" nhưng DB lưu tên tiếng Việt.
                var paymentMethod = string.Equals(req.PaymentMethod, "Cash", StringComparison.OrdinalIgnoreCase)
                    ? "Tiền mặt"
                    : req.PaymentMethod;
                req.PaymentMethod = paymentMethod;

                var (registrationId, discountAmount) = _repo.Register(req);
                return (true, "Package registered successfully", registrationId, discountAmount);
            }
            catch (Exception ex)
            {
                return (false, "Error: " + ex.Message, 0, 0);
            }
        }

        public void Pause(PausePackageRequest req) => _repo.Pause(req);

        public void Resume(int pauseId) => _repo.Resume(pauseId);

        public List<ExpiringMemberItem> ExpiringSoon(int days)
        {
            var dt = _repo.ExpiringSoon(days);
            return DtoMapper.MapList(dt, row => new ExpiringMemberItem
            {
                MemberId = DtoMapper.GetInt(row, "MemberId", "MaHV"),
                FullName = DtoMapper.GetString(row, "FullName", "HoTen") ?? string.Empty,
                Phone = DtoMapper.GetString(row, "PhoneNumber", "Phone", "SDT"),
                PackageName = DtoMapper.GetString(row, "PackageName", "TenGoi"),
                StartDate = DtoMapper.GetNullableDate(row, "StartDate", "NgayBatDau"),
                EndDate = DtoMapper.GetNullableDate(row, "EndDate", "NgayKetThuc")
            });
        }
    }
}
