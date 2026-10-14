using Microsoft.EntityFrameworkCore;
using GymManagement.Api.Models;

namespace GymManagement.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Member> Members { get; set; }
        public DbSet<MembershipPackage> MembershipPackages { get; set; }
        public DbSet<GymClass> GymClasses { get; set; }
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<PackageRegistration> PackageRegistrations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Member>(entity =>
            {
                entity.HasKey(e => e.MemberId);
            });

            modelBuilder.Entity<MembershipPackage>(entity =>
            {
                entity.HasKey(e => e.PackageId);
                entity.Property(e => e.Price).HasPrecision(18, 2);
            });

            modelBuilder.Entity<GymClass>(entity =>
            {
                entity.HasKey(e => e.ClassId);
            });

            modelBuilder.Entity<Trainer>(entity =>
            {
                entity.HasKey(e => e.TrainerId);
            });

            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.HasKey(e => e.AttendanceId);
            });

            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(e => e.AccountId);
                entity.HasIndex(e => e.Username).IsUnique();
            });

            modelBuilder.Entity<PackageRegistration>(entity =>
            {
                entity.HasKey(e => e.RegistrationId);
                entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            });
        }
    }
}
