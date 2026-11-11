namespace GymManagement.Api.Common.Exceptions
{
    /// <summary>Dữ liệu client gửi lên không hợp lệ → 400.</summary>
    public class BadRequestException : AppException
    {
        public BadRequestException(string message) : base(message, statusCode: 400)
        {
        }
    }
}
