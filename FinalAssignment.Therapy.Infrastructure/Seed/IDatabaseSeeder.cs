namespace FinalAssignment.Therapy.Infrastructure.Seed;

public interface IDatabaseSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}