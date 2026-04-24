using FinalAssignment.Therapy.Core.Common;
using FinalAssignment.Therapy.Core.Entities;
using FinalAssignment.Therapy.Core.Interfaces;
using FinalAssignment.Therapy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using FinalAssignment.Therapy.Core.Enums;

namespace FinalAssignment.Therapy.Infrastructure.Seed;

public class DatabaseSeeder(TherapyDbContext dbContext, IPasswordHasher passwordHasher) : IDatabaseSeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        // Add robust data if DB is empty
        if (!await dbContext.Users.AnyAsync(cancellationToken))
        {
            var therapists = new List<TherapistProfile>
            {
                new TherapistProfile { Name = "Dr. Anna Nguyen", Specialty = "Trauma Recovery", PhoneNumber = "0901234567", Bio = "Specialises in trauma-informed therapy and anxiety treatment. 10+ years of clinical experience.", StartWorkTime = new TimeOnly(8, 0), EndWorkTime = new TimeOnly(17, 0), AvatarUrl = "/images/therapists/lovepik-female-doctor-image-png-image_401630436_wh1200.png", YearsOfExperience = 12, Education = "PhD in Psychology, Harvard University", Rating = 4.9, WorkingDays = "1,2,3,4,5,6,7" },
                new TherapistProfile { Name = "Dr. Liam Tran", Specialty = "Mindfulness Coaching", PhoneNumber = "0912345678", Bio = "Focuses on burnout recovery, stress management, and mindfulness-based cognitive therapy.", StartWorkTime = new TimeOnly(9, 0), EndWorkTime = new TimeOnly(18, 0), AvatarUrl = "/images/therapists/istockphoto-609056120-170667a.jpg", YearsOfExperience = 8, Education = "MSc in Clinical Psychology", Rating = 4.8, WorkingDays = "1,2,3,4,5,6,7" },
                new TherapistProfile { Name = "Dr. Mai Le", Specialty = "Child Psychology", PhoneNumber = "0923456789", Bio = "Expert in developmental psychology, helping children and teenagers navigate emotional challenges.", StartWorkTime = new TimeOnly(8, 30), EndWorkTime = new TimeOnly(16, 30), AvatarUrl = "/images/therapists/istockphoto-619770964-170667a.jpg", YearsOfExperience = 15, Education = "MD, Specialized in Pediatrics Psychology", Rating = 5.0, WorkingDays = "1,2,3,4,5,6,7" },
                new TherapistProfile { Name = "Dr. Hoang Pham", Specialty = "Couples Therapy", PhoneNumber = "0934567890", Bio = "Helps couples resolve conflicts, improve communication, and rebuild relationships.", StartWorkTime = new TimeOnly(10, 0), EndWorkTime = new TimeOnly(20, 0), AvatarUrl = "/images/therapists/Anh-bac-si-nam-7-min.jpg", YearsOfExperience = 10, Education = "MA in Family Counseling", Rating = 4.7, WorkingDays = "1,2,3,4,5,6,7" },
                new TherapistProfile { Name = "Dr. Sarah Vu", Specialty = "Cognitive Behavioral Therapy", PhoneNumber = "0945678901", Bio = "Uses CBT to treat depression, anxiety disorders, and phobias effectively.", StartWorkTime = new TimeOnly(8, 0), EndWorkTime = new TimeOnly(16, 0), AvatarUrl = "/images/therapists/portrait-smiling-senior-asian-doctor-isolated-white-background_641698-310.jpg", YearsOfExperience = 6, Education = "BSc Psychology, Certified CBT Practitioner", Rating = 4.9, WorkingDays = "1,2,3,4,5,6,7" }
            };

            await dbContext.Therapists.AddRangeAsync(therapists, cancellationToken);

            var users = new List<AppUser>
            {
                new AppUser { FullName = "System Admin", Email = "admin@therapy.local", PasswordHash = passwordHasher.Hash("Admin@123"), Role = UserRoles.Admin }
            };

            for (int i = 0; i < therapists.Count; i++)
            {
                var emailPrefix = therapists[i].Name.Split(' ').Last().ToLower() + i;
                users.Add(new AppUser
                {
                    FullName = therapists[i].Name,
                    Email = $"{emailPrefix}@therapy.local",
                    PasswordHash = passwordHasher.Hash("Therapist@123"),
                    Role = UserRoles.Therapist,
                    TherapistProfile = therapists[i]
                });
            }

            await dbContext.Users.AddRangeAsync(users, cancellationToken);

            var services = new List<TherapyService>
            {
                new TherapyService { Name = "Initial Emotional Assessment", Description = "90-minute comprehensive first session to map treatment goals and background.", DurationMinutes = 90, Price = 800000m, Category = "Assessment", ImageUrl = "https://images.unsplash.com/photo-1573497019940-1c28c88b4f3e?w=800&q=80", DetailedDescription = "The Initial Emotional Assessment is a structured 90-minute deep dive into your emotional well-being. We use evidence-based psychological tools to identify core challenges and map a clear, actionable treatment plan tailored to your specific needs.", Benefits = "Clear diagnosis,Customized treatment roadmap,Safe environment" },
                new TherapyService { Name = "Standard Therapy Session", Description = "60-minute one-on-one therapy session tailored to individual needs.", DurationMinutes = 60, Price = 500000m, Category = "Individual", ImageUrl = "https://images.unsplash.com/photo-1527689368864-3a821dbccc34?w=800&q=80", DetailedDescription = "Our standard therapy sessions provide a supportive, non-judgmental space where you can explore your thoughts and feelings. We focus on developing healthy coping mechanisms and achieving your personal mental health goals.", Benefits = "Reduced anxiety,Improved mood,Better coping skills" },
                new TherapyService { Name = "Couples Counseling", Description = "90-minute joint session focusing on communication and relationship building.", DurationMinutes = 90, Price = 1200000m, Category = "Family & Couples", ImageUrl = "https://images.unsplash.com/photo-1516534775068-ba3e7458af70?w=800&q=80", DetailedDescription = "Couples Counseling is designed to help partners improve communication, resolve recurring conflicts, and deepen their emotional connection. Whether you're facing a crisis or just want to strengthen your bond, we provide neutral ground for healthy dialogue.", Benefits = "Conflict resolution,Rebuilt trust,Enhanced emotional intimacy" },
                new TherapyService { Name = "Child Play Therapy", Description = "45-minute specialized session using play to help children express emotions.", DurationMinutes = 45, Price = 600000m, Category = "Children", ImageUrl = "https://images.unsplash.com/photo-1587654780291-39c9404d746b?w=800&q=80", DetailedDescription = "Play Therapy is a specialized approach for children ages 3-12. Because children often lack the vocabulary to express complex emotions, we use play—their natural language—to help them process trauma, anxiety, and behavioral issues.", Benefits = "Better emotional regulation,Reduced behavioral issues,Increased self-esteem" },
                new TherapyService { Name = "Group Mindfulness Workshop", Description = "120-minute shared mindfulness practice and stress coping techniques.", DurationMinutes = 120, Price = 350000m, Category = "Workshops", ImageUrl = "https://images.unsplash.com/photo-1544367567-0f2fcb009e0b?w=800&q=80", DetailedDescription = "Join our Group Mindfulness Workshop to learn practical techniques for managing stress and staying present. This interactive 2-hour session combines guided meditation, breathing exercises, and group discussion.", Benefits = "Lower stress levels,Community support,Practical daily techniques" },
                new TherapyService { Name = "EMDR Trauma Session", Description = "90-minute intensive Eye Movement Desensitization and Reprocessing therapy.", DurationMinutes = 90, Price = 1500000m, Category = "Specialized", ImageUrl = "https://images.unsplash.com/photo-1499209974431-9dddcece7f88?w=800&q=80", DetailedDescription = "EMDR is an interactive psychotherapy technique used to relieve psychological stress. It is an effective treatment for trauma and post-traumatic stress disorder (PTSD), helping the brain reprocess traumatic memories safely.", Benefits = "Trauma recovery,PTSD relief,Faster healing process" }
            };

            await dbContext.TherapyServices.AddRangeAsync(services, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Generate 50 realistic past & future appointments for charting
            var random = new Random(123);
            var appointments = new List<Appointment>();
            
            for (int i = 0; i < 50; i++)
            {
                var service = services[random.Next(services.Count)];
                var therapist = therapists[random.Next(therapists.Count)];
                
                // Random date between 30 days ago and 14 days in future
                var daysOffset = random.Next(-30, 15);
                var hour = random.Next(8, 17);
                var startDate = DateTime.UtcNow.Date.AddDays(daysOffset).AddHours(hour);
                
                var status = AppointmentStatus.Completed;
                if (daysOffset > 0) status = AppointmentStatus.Confirmed;
                if (daysOffset == 0) status = AppointmentStatus.PendingPayment;
                if (random.Next(10) == 0) status = AppointmentStatus.Cancelled;

                appointments.Add(new Appointment
                {
                    PatientName = $"Patient {i}",
                    PatientEmail = $"patient{i}@example.com",
                    PatientPhone = $"0900000{i:000}",
                    TherapistProfileId = therapist.Id,
                    TherapyServiceId = service.Id,
                    AppointmentStartUtc = startDate,
                    Notes = "Generated by seeder",
                    PriceSnapshot = service.Price,
                    PaymentStatus = status == AppointmentStatus.Completed || status == AppointmentStatus.Confirmed ? PaymentStatus.Paid : PaymentStatus.Pending,
                    Status = status,
                    TrackingCode = $"REF-{startDate:yyMMdd}-{random.Next(1000,9999)}"
                });
            }

            await dbContext.Appointments.AddRangeAsync(appointments, cancellationToken);

            var testimonials = new List<Testimonial>
            {
                new Testimonial { CustomerName = "John Doe", Content = "The service is extremely professional. The doctor was very dedicated and helped me through a crisis period.", Rating = 5, CreatedAt = DateTime.UtcNow.AddDays(-10) },
                new Testimonial { CustomerName = "Jane Smith", Content = "I've tried many places but Montra provides the safest feeling. The support team is excellent.", Rating = 5, CreatedAt = DateTime.UtcNow.AddDays(-5) },
                new Testimonial { CustomerName = "Robert Johnson", Content = "The Mindfulness Workshops are very helpful for my high-stress job.", Rating = 4, CreatedAt = DateTime.UtcNow.AddDays(-2) }
            };
            await dbContext.Testimonials.AddRangeAsync(testimonials, cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
