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

            // The existing database uses the English table and column names declared
            // on the model classes. Keep the account-to-member link explicit here.
            modelBuilder.Entity<Member>().HasKey(member => member.MemberId);
            modelBuilder.Entity<MembershipPackage>().HasKey(package => package.PackageId);
            modelBuilder.Entity<GymClass>().HasKey(gymClass => gymClass.ClassId);
            modelBuilder.Entity<Trainer>().HasKey(trainer => trainer.TrainerId);
            modelBuilder.Entity<Attendance>().HasKey(attendance => attendance.AttendanceId);
            modelBuilder.Entity<Account>().HasKey(account => account.AccountId);
            modelBuilder.Entity<PackageRegistration>().HasKey(registration => registration.RegistrationId);

            modelBuilder.Entity<Member>()
                .HasOne<Account>()
                .WithOne()
                .HasForeignKey<Member>(member => member.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Account>()
                .HasIndex(account => account.Username)
                .IsUnique();

            modelBuilder.Entity<MembershipPackage>()
                .Property(package => package.Price)
                .HasPrecision(18, 0);

            modelBuilder.Entity<PackageRegistration>()
                .Property(registration => registration.DiscountAmount)
                .HasPrecision(18, 0);
        }
    }
}
