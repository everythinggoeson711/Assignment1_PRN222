using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Montra.DAL.Context
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var password = Environment.GetEnvironmentVariable("MONTRA_DB_PASSWORD");

            if (string.IsNullOrWhiteSpace(password))
            {
                var envPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".env"));

                if (File.Exists(envPath))
                {
                    var line = File.ReadLines(envPath)
                        .FirstOrDefault(value => value.StartsWith("MONTRA_DB_PASSWORD=", StringComparison.OrdinalIgnoreCase));

                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        password = line.Split('=', 2)[1].Trim();
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException("MONTRA_DB_PASSWORD was not found for EF design-time operations.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer($"Server=localhost,14333;Database=MontraTherapyDb;User Id=sa;Password={password};TrustServerCertificate=True;MultipleActiveResultSets=true");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}