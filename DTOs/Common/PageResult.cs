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

        /// <summary>
        /// Tạo kết quả khi DB đã phân trang (truyền sẵn totalCount từ SP).
        /// </summary>
        public static PageResult<T> Of(List<T> items, int totalCount, UserQueryParameters query)
        {
            var result = new PageResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                Page = query?.Page ?? 1,
                PageSize = query?.PageSize ?? items.Count
            };
            return result;
        }

        /// <summary>
        /// Phân trang tại bộ nhớ (fallback khi SP chưa nâng cấp): cắt items theo Skip/Take.
        /// </summary>
        public static PageResult<T> FromList(List<T> allItems, UserQueryParameters query)
        {
            var query1 = query ?? new UserQueryParameters();
            var result = new PageResult<T>
            {
                TotalCount = allItems.Count,
                Page = query1.Page,
                PageSize = query1.PageSize
            };
            result.Items = allItems
                .Skip(query1.Skip)
                .Take(query1.PageSize)
                .ToList();
            return result;
        }
    }
}
