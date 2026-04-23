using Microsoft.EntityFrameworkCore;
using Montra.DAL.Entities;
using System.Security.Cryptography;
using System.Text;

namespace Montra.DAL.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Therapist> Therapists => Set<Therapist>();
        public DbSet<TherapyService> TherapyServices => Set<TherapyService>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<ReferralRequest> ReferralRequests => Set<ReferralRequest>();
        public DbSet<CenterServiceBooking> CenterServiceBookings => Set<CenterServiceBooking>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TherapyService>()
                .Property(s => s.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ReferralRequest>()
                .Property(r => r.Status)
                .HasDefaultValue("New");

            modelBuilder.Entity<ReferralRequest>()
                .HasIndex(r => r.TrackingCode)
                .IsUnique();

            modelBuilder.Entity<CenterServiceBooking>()
                .Property(b => b.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<CenterServiceBooking>()
                .Property(b => b.PaymentStatus)
                .HasDefaultValue("Pending");

            modelBuilder.Entity<CenterServiceBooking>()
                .Property(b => b.BookingStatus)
                .HasDefaultValue("PendingPayment");

            modelBuilder.Entity<CenterServiceBooking>()
                .HasIndex(b => b.TrackingCode)
                .IsUnique();

            modelBuilder.Entity<Appointment>()
                .HasIndex(a => new { a.TherapistId, a.AppointmentDate, a.TimeSlot })
                .IsUnique()
                .HasFilter("[Status] <> 3");

            modelBuilder.Entity<CenterServiceBooking>()
                .HasIndex(b => new { b.TherapistId, b.AppointmentDate, b.TimeSlot })
                .IsUnique()
                .HasFilter("[BookingStatus] = 'PendingPayment'");
        }

        public static string Hash(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes).ToLower();
        }
    }
}
