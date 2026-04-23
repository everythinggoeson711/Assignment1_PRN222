using FinalAssignment.Therapy.Application.Models;
using FinalAssignment.Therapy.Core.Interfaces;

namespace FinalAssignment.Therapy.Application.Services;

public class AuthService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher) : IAuthService
{
    public async Task<AuthenticatedUser?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim();
        var user = await unitOfWork.Users.GetByEmailAsync(email, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return null;
        }

        var isValidPassword = passwordHasher.Verify(request.Password, user.PasswordHash);
        if (!isValidPassword)
        {
            return null;
        }

        return new AuthenticatedUser
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            TherapistProfileId = user.TherapistProfileId
        };
    }
}