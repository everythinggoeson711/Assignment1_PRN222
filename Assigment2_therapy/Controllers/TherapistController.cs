using Assigment2_therapy.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Montra.BLL.Services;
using Montra.DAL.Entities;

namespace Assigment2_therapy.Controllers
{
    public class TherapistController : Controller
    {
        private readonly ITherapistService   _therapistSvc;
        private readonly IAppointmentService _appointmentSvc;

        public TherapistController(ITherapistService therapistSvc, IAppointmentService appointmentSvc)
        {
            _therapistSvc   = therapistSvc;
            _appointmentSvc = appointmentSvc;
        }

        private IActionResult? Guard() =>
            HttpContext.Session.GetString("UserRole") == "Therapist"
                ? null
                : RedirectToAction("Login", "Auth");

        private int CurrentUserId => HttpContext.Session.GetInt32("UserId") ?? 0;

        // ── Dashboard ──────────────────────────────────────────────
        public async Task<IActionResult> Dashboard()
        {
            if (Guard() is { } r) return r;

            var therapist = await _therapistSvc.GetByUserIdAsync(CurrentUserId);
            if (therapist == null) return RedirectToAction("Login", "Auth");

            var all = await _appointmentSvc.GetByTherapistIdAsync(therapist.Id);

            var vm = new TherapistDashboardViewModel
            {
                Therapist          = therapist,
                TodayAppointments  = await _appointmentSvc.GetTodayByTherapistIdAsync(therapist.Id),
                TotalAssigned      = all.Count,
                PendingCount       = all.Count(a => a.Status == AppointmentStatus.Pending),
                CompletedCount     = all.Count(a => a.Status == AppointmentStatus.Completed)
            };
            return View(vm);
        }

        // ── Appointments list ──────────────────────────────────────
        public async Task<IActionResult> Appointments(AppointmentStatus? status, DateTime? date)
        {
            if (Guard() is { } r) return r;

            var therapist = await _therapistSvc.GetByUserIdAsync(CurrentUserId);
            if (therapist == null) return RedirectToAction("Login", "Auth");

            var vm = new AppointmentListViewModel
            {
                Status       = status,
                Date         = date,
                Appointments = await _appointmentSvc.GetAllAsync(therapistId: therapist.Id, status: status, date: date)
            };
            ViewBag.TherapistId = therapist.Id;
            return View(vm);
        }

        // ── Appointment Detail ────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> AppointmentDetail(int id)
        {
            if (Guard() is { } r) return r;

            var appt = await _appointmentSvc.GetByIdAsync(id);
            if (appt == null) return NotFound();

            var therapist = await _therapistSvc.GetByUserIdAsync(CurrentUserId);
            if (therapist == null || appt.TherapistId != therapist.Id)
                return Forbid();

            return View(appt);
        }

        // ── Workflow 1 Step 2-4: Update status + notes ─────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(UpdateStatusViewModel vm)
        {
            if (Guard() is { } r) return r;

            var (ok, msg) = await _appointmentSvc.UpdateStatusAsync(vm.Id, vm.Status, vm.TherapyNotes);

            if (ok) TempData["Success"] = msg;
            else    TempData["Error"]   = msg;

            return RedirectToAction(nameof(AppointmentDetail), new { id = vm.Id });
        }
    }
}
