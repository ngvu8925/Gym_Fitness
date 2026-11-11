using System.Text;
using GymManagement.Api.Data;
using GymManagement.Api.Middleware;
using GymManagement.Api.Models;
using GymManagement.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Fail-fast: lỗi cấu hình phải hiện rõ khi khởi động thay vì lỗi mơ hồ lúc chạy.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' chưa được cấu hình. " +
        "Đặt trong user secrets: dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"<value>\"");
}
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

// Bind & register JWT Settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddScoped<JwtService>();

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

builder.Services.AddSingleton<GymManagement.Api.Repositories.DatabaseHelper>();

builder.Services.AddScoped<GymManagement.Api.Repositories.MemberRepository>();
builder.Services.AddScoped<GymManagement.Api.Services.MemberService>();
builder.Services.AddScoped<GymManagement.Api.Services.MemberAccountService>();

builder.Services.AddScoped<GymManagement.Api.Repositories.PackageRepository>();
builder.Services.AddScoped<GymManagement.Api.Services.PackageService>();

builder.Services.AddScoped<GymManagement.Api.Repositories.GymClassRepository>();
builder.Services.AddScoped<GymManagement.Api.Services.GymClassService>();

builder.Services.AddScoped<GymManagement.Api.Repositories.TrainerRepository>();
builder.Services.AddScoped<GymManagement.Api.Services.TrainerService>();

builder.Services.AddScoped<GymManagement.Api.Repositories.AttendanceRepository>();
builder.Services.AddScoped<GymManagement.Api.Services.AttendanceService>();

builder.Services.AddScoped<GymManagement.Api.Repositories.ReportRepository>();
builder.Services.AddScoped<GymManagement.Api.Services.ReportService>();

builder.Services.AddScoped<GymManagement.Api.Repositories.AccountRepository>();
builder.Services.AddScoped<GymManagement.Api.Services.AccountService>();

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

app.UseExceptionMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(corsPolicyName);

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
