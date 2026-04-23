using FinalAssignment.Therapy.Application.Services;
using FinalAssignment.Therapy.Core.Common;
using FinalAssignment.Therapy.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinalAssignment.Therapy.Web.Controllers;

[Authorize(Roles = UserRoles.Admin)]
public class AdminController(
    IDashboardService dashboardService,
    IAppointmentService appointmentService,
    IAdminCatalogService adminCatalogService) : Controller
{
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
        => View(await dashboardService.GetMetricsAsync(cancellationToken));

    [HttpGet]
    public async Task<IActionResult> Therapists(CancellationToken cancellationToken)
        => View(await adminCatalogService.GetTherapistsAsync(cancellationToken));

    [HttpGet]
    public IActionResult CreateTherapist()
        => View(new TherapistFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTherapist(TherapistFormViewModel viewModel, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(viewModel.Email))
        {
            ModelState.AddModelError(nameof(viewModel.Email), "Email is required.");
        }

        if (string.IsNullOrWhiteSpace(viewModel.Password))
        {
            ModelState.AddModelError(nameof(viewModel.Password), "Password is required.");
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var result = await adminCatalogService.CreateTherapistAsync(new()
        {
            Name = viewModel.Name,
            Specialty = viewModel.Specialty,
            PhoneNumber = viewModel.PhoneNumber,
            Bio = viewModel.Bio,
            IsActive = viewModel.IsActive,
            Email = viewModel.Email ?? string.Empty,
            Password = viewModel.Password ?? string.Empty
        }, cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(viewModel);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Therapists));
    }

    [HttpGet]
    public async Task<IActionResult> EditTherapist(int id, CancellationToken cancellationToken)
    {
        var therapist = await adminCatalogService.GetTherapistByIdAsync(id, cancellationToken);
        if (therapist is null)
        {
            return NotFound();
        }

        return View(new TherapistFormViewModel
        {
            Id = therapist.Id,
            Name = therapist.Name,
            Specialty = therapist.Specialty,
            PhoneNumber = therapist.PhoneNumber,
            Bio = therapist.Bio,
            IsActive = therapist.IsActive,
            Email = therapist.Email
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTherapist(TherapistFormViewModel viewModel, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var result = await adminCatalogService.UpdateTherapistAsync(new()
        {
            Id = viewModel.Id,
            Name = viewModel.Name,
            Specialty = viewModel.Specialty,
            PhoneNumber = viewModel.PhoneNumber,
            Bio = viewModel.Bio,
            IsActive = viewModel.IsActive
        }, cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(viewModel);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Therapists));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeactivateTherapist(int id, CancellationToken cancellationToken)
    {
        var result = await adminCatalogService.DeactivateTherapistAsync(id, cancellationToken);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Therapists));
    }

    [HttpGet]
    public async Task<IActionResult> Services(CancellationToken cancellationToken)
        => View(await adminCatalogService.GetServicesAsync(cancellationToken));

    [HttpGet]
    public IActionResult CreateService()
        => View(new TherapyServiceFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateService(TherapyServiceFormViewModel viewModel, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var result = await adminCatalogService.CreateServiceAsync(new()
        {
            Name = viewModel.Name,
            Description = viewModel.Description,
            Price = viewModel.Price,
            DurationMinutes = viewModel.DurationMinutes,
            IsActive = viewModel.IsActive
        }, cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(viewModel);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Services));
    }

    [HttpGet]
    public async Task<IActionResult> EditService(int id, CancellationToken cancellationToken)
    {
        var service = await adminCatalogService.GetServiceByIdAsync(id, cancellationToken);
        if (service is null)
        {
            return NotFound();
        }

        return View(new TherapyServiceFormViewModel
        {
            Id = service.Id,
            Name = service.Name,
            Description = service.Description,
            Price = service.Price,
            DurationMinutes = service.DurationMinutes,
            IsActive = service.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditService(TherapyServiceFormViewModel viewModel, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var result = await adminCatalogService.UpdateServiceAsync(new()
        {
            Id = viewModel.Id,
            Name = viewModel.Name,
            Description = viewModel.Description,
            Price = viewModel.Price,
            DurationMinutes = viewModel.DurationMinutes,
            IsActive = viewModel.IsActive
        }, cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(viewModel);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Services));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeactivateService(int id, CancellationToken cancellationToken)
    {
        var result = await adminCatalogService.DeactivateServiceAsync(id, cancellationToken);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Services));
    }

    [HttpGet]
    public async Task<IActionResult> Metrics(CancellationToken cancellationToken)
        => Json(await dashboardService.GetMetricsAsync(cancellationToken));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmPayment(int id, CancellationToken cancellationToken)
    {
        var result = await appointmentService.ConfirmPaymentAsync(id, cancellationToken);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Dashboard));
    }
}