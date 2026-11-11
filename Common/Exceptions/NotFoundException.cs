namespace GymManagement.Api.Common.Exceptions
{
    /// <summary>Không tìm thấy tài nguyên → 404.</summary>
    public class NotFoundException : AppException
    {
        public NotFoundException(string message) : base(message, statusCode: 404)
        {
        }
    }
}
