using Assigment2_therapy.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Montra.BLL.Services;
using Montra.DAL.Context;
using Montra.DAL.Entities;

namespace Assigment2_therapy.Controllers
{
    public class AdminController : Controller
    {
        private readonly ITherapistService    _therapistSvc;
        private readonly ITherapyServiceBLL   _serviceSvc;
        private readonly IAppointmentService  _appointmentSvc;
        private readonly AppDbContext         _dbContext;

        public AdminController(ITherapistService therapistSvc, ITherapyServiceBLL serviceSvc, IAppointmentService appointmentSvc, AppDbContext dbContext)
        {
            _therapistSvc   = therapistSvc;
            _serviceSvc     = serviceSvc;
            _appointmentSvc = appointmentSvc;
            _dbContext      = dbContext;
        }

        // Returns a redirect if the current session is not Admin
        private IActionResult? Guard() =>
            HttpContext.Session.GetString("UserRole") == "Admin"
                ? null
                : RedirectToAction("Login", "Auth");

        // ── Dashboard ──────────────────────────────────────────────
        public async Task<IActionResult> Dashboard()
        {
            if (Guard() is { } r) return r;

            var vm = new AdminDashboardViewModel
            {
                TotalTherapists      = (await _therapistSvc.GetAllAsync()).Count,
                TotalAppointments    = (await _appointmentSvc.GetAllAsync()).Count,
                PendingAppointments  = await _appointmentSvc.CountByStatusAsync(AppointmentStatus.Pending),
                CompletedAppointments= await _appointmentSvc.CountByStatusAsync(AppointmentStatus.Completed),
                TotalRevenue         = await _appointmentSvc.TotalRevenueAsync(),
                RecentAppointments   = (await _appointmentSvc.GetAllAsync()).Take(5).ToList()
            };
            return View(vm);
        }

        // ── Therapists (Workflow 2 – Search & Filter) ──────────────
        public async Task<IActionResult> Therapists(string? search, string? specialty)
        {
            if (Guard() is { } r) return r;

            var vm = new TherapistSearchViewModel
            {
                Search     = search,
                Specialty  = specialty,
                Therapists = await _therapistSvc.GetAllAsync(search, specialty),
                Specialties= await _therapistSvc.GetSpecialtiesAsync()
            };
            return View(vm);
        }

        [HttpGet]
        public IActionResult CreateTherapist()
        {
            if (Guard() is { } r) return r;
            return View(new TherapistFormViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTherapist(TherapistFormViewModel vm)
        {
            if (Guard() is { } r) return r;

            if (string.IsNullOrWhiteSpace(vm.Email))    ModelState.AddModelError("Email",    "Vui lòng nhập email.");
            if (string.IsNullOrWhiteSpace(vm.Password)) ModelState.AddModelError("Password", "Vui lòng nhập mật khẩu.");
            if (!ModelState.IsValid) return View(vm);

            var user      = new User      { FullName = vm.Name, Email = vm.Email!, PasswordHash = vm.Password! };
            var therapist = new Therapist { Name = vm.Name, Specialty = vm.Specialty, Bio = vm.Bio, Phone = vm.Phone, IsActive = vm.IsActive };

            await _therapistSvc.CreateAsync(therapist, user);
            TempData["Success"] = "Thêm chuyên gia thành công!";
            return RedirectToAction(nameof(Therapists));
        }

        [HttpGet]
        public async Task<IActionResult> EditTherapist(int id)
        {
            if (Guard() is { } r) return r;

            var t = await _therapistSvc.GetByIdAsync(id);
            if (t == null) return NotFound();

            return View(new TherapistFormViewModel { Id = t.Id, Name = t.Name, Specialty = t.Specialty, Bio = t.Bio, Phone = t.Phone, IsActive = t.IsActive });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTherapist(TherapistFormViewModel vm)
        {
            if (Guard() is { } r) return r;
            if (!ModelState.IsValid) return View(vm);

            var t = await _therapistSvc.GetByIdAsync(vm.Id);
            if (t == null) return NotFound();

            t.Name = vm.Name; t.Specialty = vm.Specialty; t.Bio = vm.Bio; t.Phone = vm.Phone; t.IsActive = vm.IsActive;
            await _therapistSvc.UpdateAsync(t);
            TempData["Success"] = "Cập nhật chuyên gia thành công!";
            return RedirectToAction(nameof(Therapists));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTherapist(int id)
        {
            if (Guard() is { } r) return r;
            await _therapistSvc.DeleteAsync(id);
            TempData["Success"] = "Đã xóa chuyên gia.";
            return RedirectToAction(nameof(Therapists));
        }

        // ── Services CRUD ──────────────────────────────────────────
        public async Task<IActionResult> Services()
        {
            if (Guard() is { } r) return r;
            return View(await _serviceSvc.GetAllAsync());
        }

        [HttpGet]
        public IActionResult CreateService()
        {
            if (Guard() is { } r) return r;
            return View(new ServiceFormViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateService(ServiceFormViewModel vm)
        {
            if (Guard() is { } r) return r;
            if (!ModelState.IsValid) return View(vm);

            await _serviceSvc.CreateAsync(new TherapyService { Name = vm.Name, Description = vm.Description, Price = vm.Price, DurationMinutes = vm.DurationMinutes, IsActive = vm.IsActive });
            TempData["Success"] = "Thêm dịch vụ thành công!";
            return RedirectToAction(nameof(Services));
        }

        [HttpGet]
        public async Task<IActionResult> EditService(int id)
        {
            if (Guard() is { } r) return r;
            var s = await _serviceSvc.GetByIdAsync(id);
            if (s == null) return NotFound();

            return View(new ServiceFormViewModel { Id = s.Id, Name = s.Name, Description = s.Description, Price = s.Price, DurationMinutes = s.DurationMinutes, IsActive = s.IsActive });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditService(ServiceFormViewModel vm)
        {
            if (Guard() is { } r) return r;
            if (!ModelState.IsValid) return View(vm);

            var s = await _serviceSvc.GetByIdAsync(vm.Id);
            if (s == null) return NotFound();

            s.Name = vm.Name; s.Description = vm.Description; s.Price = vm.Price; s.DurationMinutes = vm.DurationMinutes; s.IsActive = vm.IsActive;
            await _serviceSvc.UpdateAsync(s);
            TempData["Success"] = "Cập nhật dịch vụ thành công!";
            return RedirectToAction(nameof(Services));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteService(int id)
        {
            if (Guard() is { } r) return r;
            await _serviceSvc.DeleteAsync(id);
            TempData["Success"] = "Đã xóa dịch vụ.";
            return RedirectToAction(nameof(Services));
        }

        // ── Appointments ───────────────────────────────────────────
        public async Task<IActionResult> Appointments(string? patientName, int? therapistId, AppointmentStatus? status, DateTime? date)
        {
            if (Guard() is { } r) return r;

            var vm = new AppointmentListViewModel
            {
                PatientName  = patientName,
                TherapistId  = therapistId,
                Status       = status,
                Date         = date,
                Appointments = await _appointmentSvc.GetAllAsync(patientName, therapistId, status, date),
                Therapists   = await _therapistSvc.GetAllAsync()
            };
            return View(vm);
        }

        public async Task<IActionResult> CenterBookings(string? bookingStatus, string? search, DateTime? appointmentDate)
        {
            if (Guard() is { } r) return r;

            var query = _dbContext.CenterServiceBookings
                .Include(b => b.Service)
                .Include(b => b.Therapist)
                .Include(b => b.Appointment)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(bookingStatus))
            {
                query = query.Where(b => b.BookingStatus == bookingStatus);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(b =>
                    b.ParticipantName.Contains(search) ||
                    b.ParticipantEmail.Contains(search) ||
                    b.ParticipantPhone.Contains(search));
            }

            if (appointmentDate.HasValue)
            {
                query = query.Where(b => b.AppointmentDate.Date == appointmentDate.Value.Date);
            }

            var allBookings = await _dbContext.CenterServiceBookings.ToListAsync();

            var vm = new CenterBookingManagementViewModel
            {
                BookingStatus = bookingStatus,
                Search = search,
                AppointmentDate = appointmentDate,
                StatusOptions = new List<string>
                {
                    "PendingPayment",
                    "Confirmed",
                    "Expired",
                    "PaymentFailed",
                    "ManualReview",
                    "Canceled"
                },
                TotalBookings = allBookings.Count,
                PendingPaymentCount = allBookings.Count(b => b.BookingStatus == "PendingPayment"),
                ConfirmedCount = allBookings.Count(b => b.BookingStatus == "Confirmed"),
                ExpiredCount = allBookings.Count(b => b.BookingStatus == "Expired"),
                PaymentFailedCount = allBookings.Count(b => b.BookingStatus == "PaymentFailed"),
                ManualReviewCount = allBookings.Count(b => b.BookingStatus == "ManualReview"),
                Bookings = await query
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync()
            };

            return View(vm);
        }

        public async Task<IActionResult> Referrals(string? status, string? search, DateTime? createdDate)
        {
            if (Guard() is { } r) return r;

            var query = _dbContext.ReferralRequests
                .Include(rf => rf.Service)
                .Include(rf => rf.PreferredTherapist)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(rf => rf.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(rf =>
                    rf.ParticipantName.Contains(search) ||
                    rf.ParticipantEmail.Contains(search) ||
                    rf.ReferrerName.Contains(search) ||
                    rf.ReferrerEmail.Contains(search));
            }

            if (createdDate.HasValue)
            {
                query = query.Where(rf => rf.CreatedAt.Date == createdDate.Value.Date);
            }

            var allReferrals = await _dbContext.ReferralRequests.ToListAsync();
            var statusOptions = new List<string> { "New", "UnderReview", "Contacted", "Scheduled", "Closed" };

            var vm = new ReferralQueueManagementViewModel
            {
                Status = status,
                Search = search,
                CreatedDate = createdDate,
                StatusOptions = statusOptions,
                TotalReferrals = allReferrals.Count,
                NewCount = allReferrals.Count(rf => rf.Status == "New"),
                UnderReviewCount = allReferrals.Count(rf => rf.Status == "UnderReview"),
                ContactedCount = allReferrals.Count(rf => rf.Status == "Contacted"),
                ScheduledCount = allReferrals.Count(rf => rf.Status == "Scheduled"),
                ClosedCount = allReferrals.Count(rf => rf.Status == "Closed"),
                Referrals = await query.OrderByDescending(rf => rf.CreatedAt).ToListAsync()
            };

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateReferralStatus(int id, string status)
        {
            if (Guard() is { } r) return r;

            var allowedStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "New", "UnderReview", "Contacted", "Scheduled", "Closed"
            };

            if (!allowedStatuses.Contains(status))
            {
                TempData["Error"] = "Trang thai referral khong hop le.";
                return RedirectToAction(nameof(Referrals));
            }

            var referral = await _dbContext.ReferralRequests.FindAsync(id);
            if (referral == null)
            {
                return NotFound();
            }

            referral.Status = status;
            await _dbContext.SaveChangesAsync();
            TempData["Success"] = $"Da cap nhat referral {referral.TrackingCode} sang {status}.";
            return RedirectToAction(nameof(Referrals));
        }

        [HttpGet]
        public async Task<IActionResult> CreateAppointment()
        {
            if (Guard() is { } r) return r;

            return View(new AppointmentFormViewModel
            {
                Therapists = await _therapistSvc.GetAllAsync(),
                Services   = await _serviceSvc.GetAllAsync()
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAppointment(AppointmentFormViewModel vm)
        {
            if (Guard() is { } r) return r;

            if (!ModelState.IsValid)
            {
                vm.Therapists = await _therapistSvc.GetAllAsync();
                vm.Services   = await _serviceSvc.GetAllAsync();
                return View(vm);
            }

            var appt = new Appointment
            {
                PatientName    = vm.PatientName,
                PatientPhone   = vm.PatientPhone,
                PatientEmail   = vm.PatientEmail,
                TherapistId    = vm.TherapistId,
                ServiceId      = vm.ServiceId,
                AppointmentDate= vm.AppointmentDate,
                TimeSlot       = vm.TimeSlot
            };

            var (ok, msg) = await _appointmentSvc.CreateAsync(appt);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, msg);
                vm.Therapists = await _therapistSvc.GetAllAsync();
                vm.Services   = await _serviceSvc.GetAllAsync();
                return View(vm);
            }

            TempData["Success"] = msg;
            return RedirectToAction(nameof(Appointments));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            if (Guard() is { } r) return r;
            await _appointmentSvc.DeleteAsync(id);
            TempData["Success"] = "Đã xóa lịch hẹn.";
            return RedirectToAction(nameof(Appointments));
        }

        // ── Reports ────────────────────────────────────────────────
        public async Task<IActionResult> Reports()
        {
            if (Guard() is { } r) return r;

            var vm = new AdminDashboardViewModel
            {
                TotalTherapists       = (await _therapistSvc.GetAllAsync()).Count,
                TotalAppointments     = (await _appointmentSvc.GetAllAsync()).Count,
                PendingAppointments   = await _appointmentSvc.CountByStatusAsync(AppointmentStatus.Pending),
                CompletedAppointments = await _appointmentSvc.CountByStatusAsync(AppointmentStatus.Completed),
                TotalRevenue          = await _appointmentSvc.TotalRevenueAsync(),
                RecentAppointments    = await _appointmentSvc.GetAllAsync()
            };
            return View(vm);
        }
    }
}
