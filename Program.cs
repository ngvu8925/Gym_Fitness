using System.Text;
using System.Threading.RateLimiting;
using GymManagement.Api.Middleware;
using GymManagement.Api.Models;
using GymManagement.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ===== Serilog: Console + file Logs/log-YYYYMMDD.txt (rolling theo ngày), đọc từ appsettings.json =====
builder.Host.UseSerilog((context, loggerConfiguration) =>
    loggerConfiguration.ReadFrom.Configuration(context.Configuration));

// Fail-fast: lỗi cấu hình phải hiện rõ khi khởi động thay vì lỗi mơ hồ lúc chạy.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' chưa được cấu hình. " +
        "Đặt trong user secrets: dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"<value>\"");
}
// Bind & register JWT Settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddScoped<JwtService>();

// Bind SecuritySettings (Task 06: khóa tài khoản + rate limiting) + throttle theo username
builder.Services.Configure<SecuritySettings>(builder.Configuration.GetSection("SecuritySettings"));
builder.Services.AddSingleton<GymManagement.Api.Common.LoginThrottle>();

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings is null || string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
{
    throw new InvalidOperationException(
        "JwtSettings chưa được cấu hình. Đặt trong user secrets: " +
        "dotnet user-secrets set \"JwtSettings:SecretKey\" \"<value>\" (và Issuer/Audience/ExpiryMinutes)");
}
var secretKey = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

// Add JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = builder.Environment.IsProduction();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings?.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings?.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddControllers();
builder.Services.AddAuthorization();

// ===== Rate limiting cho endpoint đăng nhập (Task 06) =====
// Giới hạn theo IP: fixed window 1 phút, trả HTTP 429 + Retry-After khi vượt.
// Ngưỡng rộng (mặc định 10/phút/IP) để không khóa nhầm người hợp lệ dùng chung IP.
var securitySettings = builder.Configuration.GetSection("SecuritySettings").Get<SecuritySettings>() ?? new SecuritySettings();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.Headers.RetryAfter = "60";
        await context.HttpContext.Response.WriteAsJsonAsync(
            new { message = "Too many login attempts. Please try again later." }, cancellationToken);
    };
    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter<string>(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = Math.Max(1, securitySettings.LoginRateLimitPerMinutePerIp),
                Window = TimeSpan.FromMinutes(1)
            }));
});

builder.Services.AddSingleton<GymManagement.Api.Repositories.DatabaseHelper>();

builder.Services.AddScoped<GymManagement.Api.Repositories.IMemberAccountRepository, GymManagement.Api.Repositories.MemberAccountRepository>();
builder.Services.AddScoped<GymManagement.Api.Repositories.IMemberRepository, GymManagement.Api.Repositories.MemberRepository>();
builder.Services.AddScoped<GymManagement.Api.Repositories.IPackageRepository, GymManagement.Api.Repositories.PackageRepository>();
builder.Services.AddScoped<GymManagement.Api.Repositories.IGymClassRepository, GymManagement.Api.Repositories.GymClassRepository>();
builder.Services.AddScoped<GymManagement.Api.Repositories.ITrainerRepository, GymManagement.Api.Repositories.TrainerRepository>();
builder.Services.AddScoped<GymManagement.Api.Repositories.IAttendanceRepository, GymManagement.Api.Repositories.AttendanceRepository>();
builder.Services.AddScoped<GymManagement.Api.Repositories.IReportRepository, GymManagement.Api.Repositories.ReportRepository>();
builder.Services.AddScoped<GymManagement.Api.Repositories.IAccountRepository, GymManagement.Api.Repositories.AccountRepository>();

builder.Services.AddScoped<GymManagement.Api.Services.IMemberService, GymManagement.Api.Services.MemberService>();
builder.Services.AddScoped<GymManagement.Api.Services.IMemberAccountService, GymManagement.Api.Services.MemberAccountService>();
builder.Services.AddScoped<GymManagement.Api.Services.IPackageService, GymManagement.Api.Services.PackageService>();
builder.Services.AddScoped<GymManagement.Api.Services.IGymClassService, GymManagement.Api.Services.GymClassService>();
builder.Services.AddScoped<GymManagement.Api.Services.ITrainerService, GymManagement.Api.Services.TrainerService>();
builder.Services.AddScoped<GymManagement.Api.Services.IAttendanceService, GymManagement.Api.Services.AttendanceService>();
builder.Services.AddScoped<GymManagement.Api.Services.IReportService, GymManagement.Api.Services.ReportService>();
builder.Services.AddScoped<GymManagement.Api.Services.IAccountService, GymManagement.Api.Services.AccountService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập Access Token JWT (chỉ cần paste mã token hoặc Bearer <token>):"
    });

    options.AddSecurityRequirement(document =>
    {
        var requirement = new OpenApiSecurityRequirement();
        requirement.Add(new OpenApiSecuritySchemeReference("Bearer", document), new List<string>());
        return requirement;
    });
});

builder.Services.AddCors(options =>
{
    var isDevelopment = builder.Environment.IsDevelopment();

    options.AddPolicy("AllowedOrigins", policy =>
    {
        var origins = builder.Configuration
            .GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        if (origins.Length == 0) return;

        policy.WithOrigins(origins)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    // Mọi origin chỉ cho phép ở môi trường Development.
    if (isDevelopment)
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    }
});

var corsPolicyName = builder.Environment.IsDevelopment() ? "AllowAll" : "AllowedOrigins";

var app = builder.Build();

// Middleware thứ tự: Exception bọc ngoài cùng -> RequestLogging -> Swagger/CORS/Auth.
// Nhờ vậy mọi lỗi từ các middleware/controller bên dưới đều được bắt và log.
app.UseExceptionHandling();
app.UseRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(corsPolicyName);

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
