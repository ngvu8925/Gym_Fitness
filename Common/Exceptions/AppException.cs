namespace GymManagement.Api.Common.Exceptions
{
    /// <summary>
    /// Exception nghiệp vụ gốc. Middleware sẽ map lỗi này (và lớp con) ra HTTP status tương ứng.
    /// </summary>
    public class AppException : Exception
    {
        /// <summary>Mã HTTP sẽ trả về (mặc định 400).</summary>
        public int StatusCode { get; }

        public AppException(string message, int statusCode = 400) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
