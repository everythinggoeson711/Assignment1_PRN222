using FinalAssignment.Therapy.Core.Common;
using FinalAssignment.Therapy.Core.Entities;
using FinalAssignment.Therapy.Core.Interfaces;
using FinalAssignment.Therapy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinalAssignment.Therapy.Infrastructure.Seed;

public class DatabaseSeeder(TherapyDbContext dbContext, IPasswordHasher passwordHasher) : IDatabaseSeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);

        if (!await dbContext.Users.AnyAsync(cancellationToken))
        {
            var therapistA = new TherapistProfile
            {
                Name = "Dr. Anna Nguyen",
                Specialty = "Trauma Recovery",
                PhoneNumber = "0901234567",
                Bio = "Specialises in trauma-informed therapy and anxiety treatment."
            };

            var therapistB = new TherapistProfile
            {
                Name = "Dr. Liam Tran",
                Specialty = "Mindfulness Coaching",
                PhoneNumber = "0912345678",
                Bio = "Focuses on burnout recovery and stress management."
            };

            await dbContext.Therapists.AddRangeAsync([therapistA, therapistB], cancellationToken);

            await dbContext.Users.AddRangeAsync(
            [
                new AppUser
                {
                    FullName = "System Admin",
                    Email = "admin@therapy.local",
                    PasswordHash = passwordHasher.Hash("Admin@123"),
                    Role = UserRoles.Admin
                },
                new AppUser
                {
                    FullName = therapistA.Name,
                    Email = "anna@therapy.local",
                    PasswordHash = passwordHasher.Hash("Therapist@123"),
                    Role = UserRoles.Therapist,
                    TherapistProfile = therapistA
                },
                new AppUser
                {
                    FullName = therapistB.Name,
                    Email = "liam@therapy.local",
                    PasswordHash = passwordHasher.Hash("Therapist@123"),
                    Role = UserRoles.Therapist,
                    TherapistProfile = therapistB
                }
            ], cancellationToken);

            await dbContext.TherapyServices.AddRangeAsync(
            [
                new TherapyService
                {
                    Name = "Initial Emotional Assessment",
                    Description = "90-minute first session to map treatment goals.",
                    DurationMinutes = 90,
                    Price = 700000m
                },
                new TherapyService
                {
                    Name = "Weekly Therapy Session",
                    Description = "60-minute one-on-one therapy session.",
                    DurationMinutes = 60,
                    Price = 500000m
                },
                new TherapyService
                {
                    Name = "Group Mindfulness Workshop",
                    Description = "Shared mindfulness practice and coping techniques.",
                    DurationMinutes = 120,
                    Price = 350000m
                }
            ], cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}