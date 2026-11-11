namespace GymManagement.Api.Models
{
    /// <summary>
    /// Định dạng JSON thống nhất cho MỌI lỗi trả về từ API.
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>Mã HTTP (400, 401, 404, 500...).</summary>
        public int StatusCode { get; set; }

        /// <summary>Thông báo lỗi hiển thị cho người dùng.</summary>
        public string Message { get; set; } = "Đã xảy ra lỗi.";

        /// <summary>
        /// Chi tiết kỹ thuật (stack trace) — CHỈ điền ở môi trường Development.
        /// </summary>
        public string? Detail { get; set; }

        /// <summary>TraceId của request — dùng để tra cứu trong file log.</summary>
        public string? TraceId { get; set; }

        /// <summary>Thời điểm xảy ra lỗi (UTC).</summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
