namespace GymManagement.Api.DTOs.Common
{
    /// <summary>
    /// Kết quả phân trang chung cho mọi endpoint list.
    /// </summary>
    public class PageResult<T>
    {
        public List<T> Items { get; set; } = new();

        /// <summary>Tổng số bản ghi (không tính phân trang).</summary>
        public int TotalCount { get; set; }

        /// <summary>Trang hiện tại (bắt đầu từ 1).</summary>
        public int Page { get; set; } = 1;

        /// <summary>Số bản ghi mỗi trang.</summary>
        public int PageSize { get; set; } = 10;

        /// <summary>Tổng số trang.</summary>
        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

        /// <summary>Có trang tiếp theo không.</summary>
        public bool HasNext => Page < TotalPages;

        /// <summary>Có trang trước không.</summary>
        public bool HasPrevious => Page > 1;
    }
}
