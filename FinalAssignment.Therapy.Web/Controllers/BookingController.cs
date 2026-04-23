using FinalAssignment.Therapy.Application.Models;
using FinalAssignment.Therapy.Application.Services;
using FinalAssignment.Therapy.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinalAssignment.Therapy.Web.Controllers;

[AllowAnonymous]
public class BookingController(ITherapyLookupService therapyLookupService, IAppointmentService appointmentService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
        => View(await BuildViewModelAsync(new BookingFormViewModel(), cancellationToken));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingFormViewModel viewModel, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(await BuildViewModelAsync(viewModel, cancellationToken));
        }

        var result = await appointmentService.BookAsync(new AppointmentBookingRequest
        {
            PatientName = viewModel.PatientName,
            PatientEmail = viewModel.PatientEmail,
            PatientPhone = viewModel.PatientPhone,
            TherapistProfileId = viewModel.TherapistProfileId,
            TherapyServiceId = viewModel.TherapyServiceId,
            AppointmentStartLocal = viewModel.AppointmentStartLocal,
            Notes = viewModel.Notes
        }, cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(await BuildViewModelAsync(viewModel, cancellationToken));
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Success), new { id = result.EntityId });
    }

    [HttpGet]
    public IActionResult Success(int id)
    {
        ViewData["AppointmentId"] = id;
        return View();
    }

    private async Task<BookingFormViewModel> BuildViewModelAsync(BookingFormViewModel viewModel, CancellationToken cancellationToken)
    {
        viewModel.AvailableTherapists = await therapyLookupService.GetActiveTherapistsAsync(cancellationToken);
        viewModel.AvailableServices = await therapyLookupService.GetActiveServicesAsync(cancellationToken);
        return viewModel;
    }
}