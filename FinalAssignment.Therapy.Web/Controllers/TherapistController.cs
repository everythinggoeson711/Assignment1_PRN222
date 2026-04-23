using FinalAssignment.Therapy.Application.Services;
using FinalAssignment.Therapy.Core.Common;
using FinalAssignment.Therapy.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinalAssignment.Therapy.Web.Controllers;

[Authorize(Roles = UserRoles.Therapist)]
public class TherapistController(IAppointmentService appointmentService) : Controller
{
    public async Task<IActionResult> MyAppointments(CancellationToken cancellationToken)
    {
        var therapistProfileId = User.GetTherapistProfileId();
        if (!therapistProfileId.HasValue)
        {
            return Forbid();
        }

        var appointments = await appointmentService.GetForTherapistAsync(therapistProfileId.Value, cancellationToken);
        return View(appointments);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id, CancellationToken cancellationToken)
    {
        var therapistProfileId = User.GetTherapistProfileId();
        if (!therapistProfileId.HasValue)
        {
            return Forbid();
        }

        var result = await appointmentService.CompleteAsync(id, therapistProfileId.Value, cancellationToken);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(MyAppointments));
    }
}