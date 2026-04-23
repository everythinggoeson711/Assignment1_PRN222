using FinalAssignment.Therapy.Application.Models;
using FinalAssignment.Therapy.Application.Services;
using FinalAssignment.Therapy.Core.Common;
using FinalAssignment.Therapy.Web.Services;
using FinalAssignment.Therapy.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinalAssignment.Therapy.Web.Controllers;

public class AuthController(IAuthService authService, IUserClaimsPrincipalFactory claimsPrincipalFactory) : Controller
{
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToRoleHome();
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel viewModel, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var user = await authService.LoginAsync(new LoginRequest
        {
            Email = viewModel.Email,
            Password = viewModel.Password
        }, cancellationToken);

        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
            return View(viewModel);
        }

        var principal = claimsPrincipalFactory.Create(user);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        if (!string.IsNullOrWhiteSpace(viewModel.ReturnUrl) && Url.IsLocalUrl(viewModel.ReturnUrl))
        {
            return Redirect(viewModel.ReturnUrl);
        }

        return RedirectToRoleHome(user.Role);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    private IActionResult RedirectToRoleHome(string? role = null)
    {
        var effectiveRole = role ?? User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        return effectiveRole == UserRoles.Admin
            ? RedirectToAction("Dashboard", "Admin")
            : RedirectToAction("MyAppointments", "Therapist");
    }
}