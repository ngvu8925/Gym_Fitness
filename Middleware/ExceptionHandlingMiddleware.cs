using System.Text.Json;
using GymManagement.Api.Common.Exceptions;
using GymManagement.Api.Models;

namespace GymManagement.Api.Middleware
{
    /// <summary>
    /// Bọc toàn bộ request: bắt exception, map ra HTTP status, ghi log và
    /// trả JSON ErrorResponse thống nhất cho mọi lỗi.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var (statusCode, message, isBusinessError) = ex switch
            {
                BadRequestException e => (400, e.Message, true),
                NotFoundException e => (404, e.Message, true),
                UnauthorizedException e => (401, e.Message, true),
                AppException e => (e.StatusCode, e.Message, true),
                _ => (500, "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.", false)
            };

            // Lỗi nghiệp vụ (do code chủ động throw) => Warning; lỗi hệ thống => Error.
            if (isBusinessError)
                _logger.LogWarning("Lỗi nghiệp vụ {StatusCode} khi xử lý {Method} {Path}: {Message}",
                    statusCode, context.Request.Method, context.Request.Path, ex.Message);
            else
                _logger.LogError(ex, "Unhandled exception {StatusCode} khi xử lý {Method} {Path}",
                    statusCode, context.Request.Method, context.Request.Path);

            var response = new ErrorResponse
            {
                StatusCode = statusCode,
                Message = message,
                // Stack trace chỉ hiện ở Development để không lộ thông tin nhạy cảm.
                Detail = _env.IsDevelopment() ? ex.ToString() : null,
                TraceId = context.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    // Không serialize Detail = null, giữ JSON gọn ở Production.
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                }));
        }
    }

    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
