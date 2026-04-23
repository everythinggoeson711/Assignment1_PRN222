namespace FinalAssignment.Therapy.Core.Interfaces;

public interface IClock
{
    DateTime UtcNow { get; }
}