using System.Data;
using GymManagement.Api.DTOs;
using GymManagement.Api.DTOs.Common;
using GymManagement.Api.Repositories;

namespace GymManagement.Api.Services
{
    public class MemberService : IMemberService
    {
        private readonly IMemberRepository _repo;

        public MemberService(IMemberRepository repo) => _repo = repo;

        public (bool Success, string Message, int MemberId) Add(MemberRequest member)
        {
            if (string.IsNullOrWhiteSpace(member.FullName))
                return (false, "Full name is required", 0);
            if (string.IsNullOrWhiteSpace(member.Phone))
                return (false, "Phone number is required", 0);

            try
            {
                int memberId = _repo.Add(member);
                return (true, "Member added successfully", memberId);
            }
            catch (Exception ex)
            {
                return (false, "Error: " + ex.Message, 0);
            }
        }

        public (bool Success, string Message) Update(int memberId, MemberRequest member)
        {
            if (string.IsNullOrWhiteSpace(member.FullName))
                return (false, "Full name is required");

            _repo.Update(memberId, member);
            return (true, "Updated successfully");
        }

        public void Delete(int memberId) => _repo.Delete(memberId);

        public MemberListItem? GetById(int memberId)
        {
            var dt = _repo.GetById(memberId);
            return dt.Rows.Count == 0 ? null : MapMember(dt.Rows[0]);
        }

        public PageResult<MemberListItem> Search(string? keyword, UserQueryParameters query)
        {
            var dt = _repo.Search(keyword, query);
            var items = DtoMapper.MapList(dt, MapMember);

            // SP đã phân trang ở DB (có cột TotalCount)
            if (dt.Columns.Contains("TotalCount") && dt.Rows.Count > 0)
                return PageResult<MemberListItem>.Of(items, Convert.ToInt32(dt.Rows[0]["TotalCount"]), query);

            // Fallback: SP cũ trả toàn bộ => lọc + cắt tại bộ nhớ
            var search = query.NormalizedKeyword;
            if (search != null)
            {
                items = items.Where(m =>
                    m.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (m.Phone?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
            }
            var sorted = query.IsDescending
                ? items.OrderByDescending(m => m.FullName).ToList()
                : items.OrderBy(m => m.FullName).ToList();
            return PageResult<MemberListItem>.FromList(sorted, query);
        }

        private static MemberListItem MapMember(DataRow row) => new()
        {
            MemberId = DtoMapper.GetInt(row, "MemberId", "MaHV"),
            BranchId = DtoMapper.GetNullableInt(row, "BranchId", "MaCN"),
            BranchName = DtoMapper.GetString(row, "BranchName", "TenCN"),
            FullName = DtoMapper.GetString(row, "FullName", "HoTen") ?? string.Empty,
            Gender = DtoMapper.GetString(row, "Gender", "GioiTinh"),
            DateOfBirth = DtoMapper.GetNullableDate(row, "DateOfBirth", "NgaySinh"),
            Phone = DtoMapper.GetString(row, "PhoneNumber", "Phone", "SDT") ?? string.Empty,
            Email = DtoMapper.GetString(row, "Email"),
            Address = DtoMapper.GetString(row, "Address", "DiaChi"),
            Avatar = DtoMapper.GetString(row, "ProfileImage", "AnhDaiDien"),
            ReferrerId = DtoMapper.GetNullableInt(row, "ReferrerMemberId", "MaNguoiGioiThieu"),
            JoinDate = DtoMapper.GetNullableDate(row, "JoinDate", "NgayThamGia"),
            Status = DtoMapper.GetNullableBool(row, "Status", "TrangThai")
        };
    }
}
