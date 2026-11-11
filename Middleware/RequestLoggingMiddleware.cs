namespace GymManagement.Api.Middleware
{
    /// <summary>
    /// Ghi log mỗi request: method, path, query string, status code,
    /// thời gian xử lý, user đăng nhập và TraceId.
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                var user = context.User?.Identity?.IsAuthenticated == true
                    ? context.User.Identity!.Name ?? "authenticated"
                    : "anonymous";

                var logLevel = context.Response.StatusCode >= 500 ? LogLevel.Error
                    : context.Response.StatusCode >= 400 ? LogLevel.Warning
                    : LogLevel.Information;

                _logger.Log(logLevel,
                    "HTTP {Method} {Path}{Query} => {StatusCode} ({Elapsed} ms) | User: {User} | TraceId: {TraceId}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Request.QueryString.HasValue ? context.Request.QueryString.Value : string.Empty,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    user,
                    context.TraceIdentifier);
            }
        }
    }

    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}
