namespace GymManagement.Api.Common.Exceptions
{
    /// <summary>Chưa đăng nhập / không đủ quyền → 401.</summary>
    public class UnauthorizedException : AppException
    {
        public UnauthorizedException(string message) : base(message, statusCode: 401)
        {
        }
    }
}
