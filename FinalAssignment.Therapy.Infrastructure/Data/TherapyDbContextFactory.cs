using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FinalAssignment.Therapy.Infrastructure.Data;

public class TherapyDbContextFactory : IDesignTimeDbContextFactory<TherapyDbContext>
{
    public TherapyDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TherapyDbContext>();
        optionsBuilder.UseSqlServer("Server=localhost,14335;Database=FinalAssignmentTherapy;User Id=sa;Password=Your_password123;TrustServerCertificate=True");
        return new TherapyDbContext(optionsBuilder.Options);
    }
}