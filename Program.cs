using Microsoft.EntityFrameworkCore;
using GymManagement.Api.Data;
using GymManagement.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddControllers();

builder.Services.AddSingleton<GymManagement.Api.Repositories.DatabaseHelper>();

builder.Services.AddScoped<GymManagement.Api.Repositories.MemberRepository>();
builder.Services.AddScoped<GymManagement.Api.Services.MemberService>();

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
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseExceptionMiddleware();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
