using Assigment2_therapy.Models;
using Assigment2_therapy.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Montra.BLL.Services;
using Montra.DAL.Context;
using Montra.DAL.Entities;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;
using System.Text;

namespace Assigment2_therapy.Controllers
{
    public class HomeController : Controller
    {
        private readonly ITherapyServiceBLL _serviceSvc;
        private readonly ITherapistService _therapistSvc;
        private readonly IReferralService _referralSvc;
        private readonly ICenterBookingService _centerBookingSvc;
        private readonly CenterBookingSettings _centerBookingSettings;
        private readonly AppDbContext _dbContext;

        public HomeController(ITherapyServiceBLL serviceSvc, ITherapistService therapistSvc, IReferralService referralSvc, ICenterBookingService centerBookingSvc, Microsoft.Extensions.Options.IOptions<CenterBookingSettings> centerBookingSettings, AppDbContext dbContext)
        {
            _serviceSvc = serviceSvc;
            _therapistSvc = therapistSvc;
            _referralSvc = referralSvc;
            _centerBookingSvc = centerBookingSvc;
            _centerBookingSettings = centerBookingSettings.Value;
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await BuildViewModelAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReferral(HomeReferralPageViewModel vm)
        {
            vm = await BuildViewModelAsync(vm);

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Functional Capacity Assessments";
                return View("Index", vm);
            }

            var request = new ReferralRequest
            {
                ParticipantName = vm.ParticipantName,
                DateOfBirth = vm.DateOfBirth,
                NdisNumber = vm.NdisNumber,
                ParticipantPhone = vm.ParticipantPhone,
                ParticipantEmail = vm.ParticipantEmail,
                HomeAddress = vm.HomeAddress,
                ReferrerName = vm.ReferrerName,
                Organisation = vm.Organisation,
                ReferrerEmail = vm.ReferrerEmail,
                ReferrerPhone = vm.ReferrerPhone,
                ReferrerRole = vm.ReferrerRole,
                ServiceId = vm.ServiceId,
                PreferredTherapistId = vm.PreferredTherapistId,
                PrimaryReason = vm.PrimaryReason,
                MainReason = vm.MainReason,
                KeyConcerns = string.Join(", ", vm.KeyConcerns),
                DesiredOutcomes = string.Join(", ", vm.DesiredOutcomes),
                UrgencyLevel = vm.UrgencyLevel,
                PreferredContactMethod = vm.PreferredContactMethod,
                AdditionalNotes = vm.AdditionalNotes,
                PrivacyConsent = vm.PrivacyConsent
            };

            var result = await _referralSvc.CreateAsync(request);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                ViewData["Title"] = "Functional Capacity Assessments";
                return View("Index", vm);
            }

            SetGeneratedTrackingTempData("Referral", result.TrackingCode, "New");
            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index), new { submitted = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartCenterBooking(CenterBookingFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Vui long dien day du thong tin booking truoc khi thanh toan.";
                return RedirectToAction(nameof(Index), new { anchor = "booking" });
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = await _centerBookingSvc.CreateCheckoutAsync(new CenterBookingInput
            {
                ParticipantName = vm.ParticipantName,
                ParticipantEmail = vm.ParticipantEmail,
                ParticipantPhone = vm.ParticipantPhone,
                ServiceId = vm.ServiceId,
                TherapistId = vm.TherapistId,
                AppointmentDate = vm.AppointmentDate,
                TimeSlot = vm.TimeSlot,
                Notes = vm.Notes,
                BaseUrl = baseUrl
            });

            if (!result.Success || string.IsNullOrWhiteSpace(result.CheckoutUrl))
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index), new { anchor = "booking" });
            }

            return Redirect(result.CheckoutUrl);
        }

        [HttpGet]
        public async Task<IActionResult> CheckAvailability(int therapistId, DateTime appointmentDate, string timeSlot)
        {
            var result = await _centerBookingSvc.CheckAvailabilityAsync(therapistId, appointmentDate, timeSlot);
            return Json(new { available = result.Available, message = result.Message });
        }

        [HttpGet]
        public async Task<IActionResult> BookingSuccess(int bookingId)
        {
            var booking = await _dbContext.CenterServiceBookings.FirstOrDefaultAsync(b => b.Id == bookingId);
            if (booking != null)
            {
                SetGeneratedTrackingTempData("Center booking", booking.TrackingCode, booking.BookingStatus);
            }

            TempData["Success"] = $"Thanh toan thanh cong cho booking #{bookingId}. Montra se xac nhan lich hen ngay khi webhook hoan tat.";
            return RedirectToAction(nameof(Index), new { anchor = "booking" });
        }

        [HttpGet]
        public async Task<IActionResult> BookingCanceled(int bookingId)
        {
            await _centerBookingSvc.MarkCheckoutCanceledAsync(bookingId);
            var booking = await _dbContext.CenterServiceBookings.FirstOrDefaultAsync(b => b.Id == bookingId);
            if (booking != null)
            {
                SetGeneratedTrackingTempData("Center booking", booking.TrackingCode, booking.BookingStatus);
            }

            TempData["Error"] = "Ban da huy thanh toan booking. Khung gio se khong duoc giu nua.";
            return RedirectToAction(nameof(Index), new { anchor = "booking" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TrackRequest(string trackingCode)
        {
            var vm = await BuildViewModelAsync();
            vm.TrackingCodeInput = trackingCode?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(vm.TrackingCodeInput))
            {
                vm.TrackingLookupError = "Vui long nhap ma theo doi de xem trang thai referral hoac booking.";
                return View("Index", vm);
            }

            vm.TrackingLookup = await BuildTrackingLookupAsync(vm.TrackingCodeInput);
            if (vm.TrackingLookup == null)
            {
                vm.TrackingLookupError = "Khong tim thay yeu cau nao khop voi ma theo doi vua nhap.";
            }

            return View("Index", vm);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadTrackingReceipt(string trackingCode)
        {
            if (string.IsNullOrWhiteSpace(trackingCode))
            {
                return BadRequest();
            }

            var receiptContent = await BuildTrackingReceiptAsync(trackingCode.Trim());
            if (receiptContent == null)
            {
                return NotFound();
            }

            var fileName = $"{trackingCode.Trim().Replace(' ', '-')}.txt";
            return File(Encoding.UTF8.GetBytes(receiptContent), "text/plain", fileName);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> StripeWebhook()
        {
            if (string.IsNullOrWhiteSpace(_centerBookingSettings.WebhookSecret))
            {
                return BadRequest();
            }

            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            var payload = await reader.ReadToEndAsync();
            var signatureHeader = Request.Headers["Stripe-Signature"];

            Event stripeEvent;

            try
            {
                stripeEvent = EventUtility.ConstructEvent(payload, signatureHeader, _centerBookingSettings.WebhookSecret);
            }
            catch (Exception)
            {
                return BadRequest();
            }

            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Session;
                if (session != null)
                {
                    await _centerBookingSvc.MarkCheckoutCompletedAsync(session.Id, session.PaymentIntentId ?? string.Empty);
                }
            }
            else if (stripeEvent.Type == "checkout.session.expired")
            {
                var session = stripeEvent.Data.Object as Session;
                if (session != null)
                {
                    await _centerBookingSvc.MarkCheckoutExpiredAsync(session.Id);
                }
            }
            else if (stripeEvent.Type == "payment_intent.payment_failed")
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                if (paymentIntent != null)
                {
                    int? bookingId = null;
                    if (paymentIntent.Metadata != null && paymentIntent.Metadata.TryGetValue("bookingId", out var bookingIdValue) && int.TryParse(bookingIdValue, out var parsedBookingId))
                    {
                        bookingId = parsedBookingId;
                    }

                    await _centerBookingSvc.MarkCheckoutFailedAsync(bookingId, paymentIntent.Id, paymentIntent.LastPaymentError?.Message ?? "Stripe payment failed.");
                }
            }

            return Ok();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        private async Task<HomeReferralPageViewModel> BuildViewModelAsync(HomeReferralPageViewModel? source = null)
        {
            var vm = source ?? new HomeReferralPageViewModel();
            vm.Services = (await _serviceSvc.GetAllAsync()).Where(s => s.IsActive).ToList();
            vm.Therapists = (await _therapistSvc.GetAllAsync()).Where(t => t.IsActive).ToList();
            vm.ActiveServiceCount = vm.Services.Count;
            vm.ActiveTherapistCount = vm.Therapists.Count;
            vm.ReferralQueueCount = await _dbContext.ReferralRequests.CountAsync(r => r.Status == "New");
            vm.ConfirmedAppointmentCount = await _dbContext.Appointments.CountAsync(a => a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Completed);

            if (vm.ServiceId == 0 && vm.Services.Count > 0)
            {
                vm.ServiceId = vm.Services[0].Id;
            }

            ViewData["Title"] = "Functional Capacity Assessments";
            ViewData["StripePublishableKey"] = _centerBookingSettings.PublishableKey;
            ViewData["BookingTimeSlots"] = AppointmentFormViewModel.TimeSlots;
            return vm;
        }

        private void SetGeneratedTrackingTempData(string itemType, string trackingCode, string status)
        {
            TempData["GeneratedTrackingType"] = itemType;
            TempData["GeneratedTrackingCode"] = trackingCode;
            TempData["GeneratedTrackingStatus"] = status;
        }

        private async Task<PublicTrackingLookupViewModel?> BuildTrackingLookupAsync(string trackingCode)
        {
            trackingCode = trackingCode.Trim().ToUpperInvariant();

            var referral = await _dbContext.ReferralRequests
                .Include(r => r.Service)
                .Include(r => r.PreferredTherapist)
                .FirstOrDefaultAsync(r => r.TrackingCode == trackingCode);

            if (referral != null)
            {
                return new PublicTrackingLookupViewModel
                {
                    TrackingCode = referral.TrackingCode,
                    TrackingType = "Referral intake",
                    PrimaryStatus = referral.Status,
                    SecondaryStatus = $"Submitted {referral.CreatedAt:dd/MM/yyyy HH:mm}",
                    ParticipantName = referral.ParticipantName,
                    ContactLine = $"Participant: {referral.ParticipantEmail} | Referrer: {referral.ReferrerEmail}",
                    ServiceName = referral.Service?.Name ?? "N/A",
                    ClinicianLine = referral.PreferredTherapist != null ? $"Preferred clinician: {referral.PreferredTherapist.Name}" : "Preferred clinician: Best matched clinician",
                    TimelineLine = $"Urgency: {referral.UrgencyLevel} | Contact method: {referral.PreferredContactMethod}",
                    SupportingLine = $"Primary reason: {referral.PrimaryReason}",
                    NotesLine = string.IsNullOrWhiteSpace(referral.MainReason) ? "No additional clinical summary provided." : referral.MainReason
                };
            }

            var booking = await _dbContext.CenterServiceBookings
                .Include(b => b.Service)
                .Include(b => b.Therapist)
                .Include(b => b.Appointment)
                .FirstOrDefaultAsync(b => b.TrackingCode == trackingCode);

            if (booking == null)
            {
                return null;
            }

            return new PublicTrackingLookupViewModel
            {
                TrackingCode = booking.TrackingCode,
                TrackingType = "Center booking",
                PrimaryStatus = booking.BookingStatus,
                SecondaryStatus = $"Payment: {booking.PaymentStatus}",
                ParticipantName = booking.ParticipantName,
                ContactLine = $"Participant: {booking.ParticipantEmail}",
                ServiceName = booking.Service?.Name ?? "N/A",
                ClinicianLine = booking.Therapist != null ? $"Clinician: {booking.Therapist.Name}" : "Clinician: N/A",
                TimelineLine = $"{booking.AppointmentDate:dd/MM/yyyy} | {booking.TimeSlot}",
                SupportingLine = booking.AppointmentId.HasValue ? $"Confirmed appointment ID: {booking.AppointmentId.Value}" : "Appointment has not been created yet.",
                NotesLine = string.IsNullOrWhiteSpace(booking.Notes) ? "No additional notes were provided." : booking.Notes
            };
        }

        private async Task<string?> BuildTrackingReceiptAsync(string trackingCode)
        {
            var tracking = await BuildTrackingLookupAsync(trackingCode);
            if (tracking == null)
            {
                return null;
            }

            var builder = new StringBuilder();
            builder.AppendLine("Montra Therapy Tracking Receipt");
            builder.AppendLine($"Tracking code: {tracking.TrackingCode}");
            builder.AppendLine($"Type: {tracking.TrackingType}");
            builder.AppendLine($"Primary status: {tracking.PrimaryStatus}");
            builder.AppendLine($"Secondary status: {tracking.SecondaryStatus}");
            builder.AppendLine($"Participant: {tracking.ParticipantName}");
            builder.AppendLine($"Contact: {tracking.ContactLine}");
            builder.AppendLine($"Service: {tracking.ServiceName}");
            builder.AppendLine($"Clinician: {tracking.ClinicianLine}");
            builder.AppendLine($"Timeline: {tracking.TimelineLine}");
            builder.AppendLine($"Supporting info: {tracking.SupportingLine}");
            builder.AppendLine($"Notes: {tracking.NotesLine}");
            builder.AppendLine("Keep this code safe to check your referral or booking again later.");
            return builder.ToString();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
