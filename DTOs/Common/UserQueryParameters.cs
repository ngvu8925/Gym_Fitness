namespace GymManagement.Api.DTOs.Common
{
    /// <summary>
    /// Tham số truy vấn chung cho các endpoint list: phân trang + tìm kiếm + sắp xếp.
    /// Bind từ query string, ví dụ: ?page=1&amp;pageSize=10&amp;keyword=nguyen&amp;sortBy=name&amp;sortDir=asc
    /// </summary>
    public class UserQueryParameters
    {
        private const int MaxPageSize = 100;

        private int _page = 1;
        private int _pageSize = 10;

        /// <summary>Trang hiện tại (bắt đầu từ 1, tối thiểu 1).</summary>
        public int Page
        {
            get => _page;
            set => _page = value < 1 ? 1 : value;
        }

        /// <summary>Số bản ghi mỗi trang (1-100, mặc định 10).</summary>
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? 10 : (value > MaxPageSize ? MaxPageSize : value);
        }

        /// <summary>Từ khóa tìm kiếm (tùy chọn).</summary>
        public string? Keyword { get; set; }

        /// <summary>Cột sắp xếp (tùy chọn, VD: name, date).</summary>
        public string? SortBy { get; set; }

        /// <summary>Chiều sắp xếp: asc | desc (mặc định asc).</summary>
        public string SortDir { get; set; } = "asc";

        /// <summary>Số bản ghi cần bỏ qua khi truy vấn DB.</summary>
        public int Skip => (Page - 1) * PageSize;

        /// <summary>Chiều sắp xếp là giảm dần không.</summary>
        public bool IsDescending => string.Equals(SortDir, "desc", StringComparison.OrdinalIgnoreCase);

        /// <summary>Keyword chuẩn hóa để tìm kiếm LIKE (trim + bỏ rỗng).</summary>
        public string? NormalizedKeyword => string.IsNullOrWhiteSpace(Keyword) ? null : Keyword.Trim();
    }
}
