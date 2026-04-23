namespace FinalAssignment.Therapy.Application.Models;

public class ServiceResult
{
    public bool Succeeded { get; init; }

    public string Message { get; init; } = string.Empty;

    public int? EntityId { get; init; }

    public static ServiceResult Success(string message, int? entityId = null)
        => new() { Succeeded = true, Message = message, EntityId = entityId };

    public static ServiceResult Failure(string message)
        => new() { Succeeded = false, Message = message };
}