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
    public async Task<IActionResult> Create(int? serviceId, CancellationToken cancellationToken)
    {
        var viewModel = new BookingFormViewModel();
        if (serviceId.HasValue)
        {
            viewModel.TherapyServiceId = serviceId.Value;
        }
        return View(await BuildViewModelAsync(viewModel, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingFormViewModel viewModel, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { succeeded = false, message = "Please fix the validation errors.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }
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

        if (result.Succeeded && result.EntityId.HasValue)
        {
            // Automatically confirm payment as requested
            await appointmentService.ConfirmPaymentAsync(result.EntityId.Value, cancellationToken);
        }

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new { succeeded = result.Succeeded, message = result.Succeeded ? "Appointment confirmed and paid automatically." : result.Message, entityId = result.EntityId });
        }

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(await BuildViewModelAsync(viewModel, cancellationToken));
        }

        return RedirectToAction(nameof(BookingComplete), new { id = result.EntityId });
    }

    [HttpGet]
    public async Task<IActionResult> Confirm(int id, CancellationToken cancellationToken)
    {
        var appointment = await appointmentService.GetAppointmentDetailsAsync(id, cancellationToken);
        if (appointment is null)
        {
            return NotFound();
        }
        return View(appointment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SimulatePayment(int appointmentId, CancellationToken cancellationToken)
    {
        var result = await appointmentService.ConfirmPaymentAsync(appointmentId, cancellationToken);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(BookingComplete), new { id = appointmentId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult PayLater(int appointmentId)
    {
        TempData["Success"] = "Your appointment is pending. Please complete the payment to confirm your session.";
        return RedirectToAction(nameof(BookingComplete), new { id = appointmentId });
    }

    [HttpGet]
    public async Task<IActionResult> BookingComplete(int id, CancellationToken cancellationToken)
    {
        var appointment = await appointmentService.GetAppointmentDetailsAsync(id, cancellationToken);
        if (appointment is null)
        {
            return NotFound();
        }
        return View(appointment);
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