using System.Security.Claims;
using FinalAssignment.Therapy.Application.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace FinalAssignment.Therapy.Web.Services;

public interface IUserClaimsPrincipalFactory
{
    ClaimsPrincipal Create(AuthenticatedUser user);
}

public class CookieUserClaimsPrincipalFactory : IUserClaimsPrincipalFactory
{
    public ClaimsPrincipal Create(AuthenticatedUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        if (user.TherapistProfileId.HasValue)
        {
            claims.Add(new Claim("TherapistProfileId", user.TherapistProfileId.Value.ToString()));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }
}