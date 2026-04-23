using FinalAssignment.Therapy.Core.Interfaces;

namespace FinalAssignment.Therapy.Infrastructure.Services;

public class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}