using FinalAssignment.Therapy.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinalAssignment.Therapy.Infrastructure.Data;

public class TherapyDbContext(DbContextOptions<TherapyDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();

    public DbSet<TherapistProfile> Therapists => Set<TherapistProfile>();

    public DbSet<TherapyService> TherapyServices => Set<TherapyService>();

    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>()
            .HasIndex(user => user.Email)
            .IsUnique();

        modelBuilder.Entity<AppUser>()
            .HasOne(user => user.TherapistProfile)
            .WithOne()
            .HasForeignKey<AppUser>(user => user.TherapistProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TherapyService>()
            .Property(service => service.Price)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Appointment>()
            .Property(appointment => appointment.PriceSnapshot)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Appointment>()
            .HasIndex(appointment => new { appointment.TherapistProfileId, appointment.AppointmentStartUtc })
            .IsUnique()
            .HasFilter("[Status] IN (0, 1, 2)");
    }
}