using FinalAssignment.Therapy.Application.Models;

namespace FinalAssignment.Therapy.Application.Services;

public interface IAuthService
{
    Task<AuthenticatedUser?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}