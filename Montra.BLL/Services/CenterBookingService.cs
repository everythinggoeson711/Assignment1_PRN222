using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Montra.DAL.Context;
using Montra.DAL.Entities;
using Montra.DAL.Repositories;
using Stripe;
using Stripe.Checkout;
using System.Data;

namespace Montra.BLL.Services
{
    public class CenterBookingService : ICenterBookingService
    {
        private readonly AppDbContext _db;
        private readonly IAppointmentRepository _appointments;
        private readonly CenterBookingSettings _settings;

        public CenterBookingService(AppDbContext db, IAppointmentRepository appointments, IOptions<CenterBookingSettings> settings)
        {
            _db = db;
            _appointments = appointments;
            _settings = settings.Value;
        }

        public async Task<CenterBookingCheckoutResult> CreateCheckoutAsync(CenterBookingInput input)
        {
            if (string.IsNullOrWhiteSpace(_settings.SecretKey))
            {
                return new CenterBookingCheckoutResult
                {
                    Success = false,
                    Message = "Stripe chua duoc cau hinh. Them STRIPE_SECRET_KEY vao .env de bat thanh toan online."
                };
            }

            var service = await _db.TherapyServices.FirstOrDefaultAsync(s => s.Id == input.ServiceId && s.IsActive);
            if (service == null)
            {
                return new CenterBookingCheckoutResult { Success = false, Message = "Dich vu khong ton tai hoac da ngung hoat dong." };
            }

            var therapist = await _db.Therapists.FirstOrDefaultAsync(t => t.Id == input.TherapistId && t.IsActive);
            if (therapist == null)
            {
                return new CenterBookingCheckoutResult { Success = false, Message = "Chuyen gia khong kha dung cho lich hen nay." };
            }

            CenterServiceBooking booking;

            await using (var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable))
            {
                await AcquireSlotLockAsync(input.TherapistId, input.AppointmentDate.Date, input.TimeSlot);
                await ExpirePendingBookingsAsync(input.TherapistId, input.AppointmentDate.Date, input.TimeSlot);

                var availability = await CheckAvailabilityCoreAsync(input.TherapistId, input.AppointmentDate.Date, input.TimeSlot);
                if (!availability.Available)
                {
                    return new CenterBookingCheckoutResult { Success = false, Message = availability.Message };
                }

                booking = new CenterServiceBooking
                {
                    ParticipantName = input.ParticipantName,
                    ParticipantEmail = input.ParticipantEmail,
                    ParticipantPhone = input.ParticipantPhone,
                    ServiceId = input.ServiceId,
                    TherapistId = input.TherapistId,
                    AppointmentDate = input.AppointmentDate.Date,
                    TimeSlot = input.TimeSlot,
                    Notes = input.Notes,
                    Amount = service.Price,
                    Currency = "vnd",
                    PaymentStatus = "Pending",
                    BookingStatus = "PendingPayment",
                    TrackingCode = await GenerateTrackingCodeAsync(),
                    CreatedAt = DateTime.Now,
                    ExpiresAt = DateTime.Now.AddMinutes(_settings.PendingPaymentExpiryMinutes)
                };

                _db.CenterServiceBookings.Add(booking);

                try
                {
                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (DbUpdateException)
                {
                    await transaction.RollbackAsync();
                    return new CenterBookingCheckoutResult
                    {
                        Success = false,
                        Message = "Khung gio nay vua duoc dat boi mot khach khac. Vui long chon khung gio khac."
                    };
                }
            }

            StripeConfiguration.ApiKey = _settings.SecretKey;

            var sessionOptions = new SessionCreateOptions
            {
                Mode = "payment",
                SuccessUrl = $"{input.BaseUrl}/Home/BookingSuccess?bookingId={booking.Id}&session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{input.BaseUrl}/Home/BookingCanceled?bookingId={booking.Id}",
                CustomerEmail = input.ParticipantEmail,
                ExpiresAt = booking.ExpiresAt?.ToUniversalTime(),
                Metadata = new Dictionary<string, string>
                {
                    ["bookingId"] = booking.Id.ToString()
                },
                PaymentIntentData = new SessionPaymentIntentDataOptions
                {
                    Metadata = new Dictionary<string, string>
                    {
                        ["bookingId"] = booking.Id.ToString()
                    }
                },
                LineItems = new List<SessionLineItemOptions>
                {
                    new()
                    {
                        Quantity = 1,
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "vnd",
                            UnitAmount = (long)Math.Round(service.Price, MidpointRounding.AwayFromZero),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"{service.Name} - Center booking",
                                Description = $"{therapist.Name} | {input.AppointmentDate:dd/MM/yyyy} | {input.TimeSlot}"
                            }
                        }
                    }
                }
            };

            try
            {
                var sessionService = new SessionService();
                var session = await sessionService.CreateAsync(sessionOptions);

                booking.StripeSessionId = session.Id;
                if (!string.IsNullOrWhiteSpace(session.PaymentIntentId))
                {
                    booking.StripePaymentIntentId = session.PaymentIntentId;
                }

                await _db.SaveChangesAsync();

                return new CenterBookingCheckoutResult
                {
                    Success = true,
                    BookingId = booking.Id,
                    TrackingCode = booking.TrackingCode,
                    CheckoutUrl = session.Url ?? string.Empty,
                    Message = "Dang chuyen den cong thanh toan."
                };
            }
            catch (Exception)
            {
                booking.PaymentStatus = "Failed";
                booking.BookingStatus = "PaymentFailed";
                booking.ExpiresAt = DateTime.Now;
                await _db.SaveChangesAsync();

                return new CenterBookingCheckoutResult
                {
                    Success = false,
                    Message = "Khong the tao phien thanh toan luc nay. Vui long thu lai sau."
                };
            }
        }

        public async Task<(bool Available, string Message)> CheckAvailabilityAsync(int therapistId, DateTime date, string timeSlot)
        {
            await ExpirePendingBookingsAsync(therapistId, date.Date, timeSlot);
            return await CheckAvailabilityCoreAsync(therapistId, date.Date, timeSlot);
        }

        public async Task MarkCheckoutCompletedAsync(string stripeSessionId, string stripePaymentIntentId)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var booking = await _db.CenterServiceBookings.FirstOrDefaultAsync(b => b.StripeSessionId == stripeSessionId);
            if (booking == null || booking.PaymentStatus == "Paid")
            {
                return;
            }

            await AcquireSlotLockAsync(booking.TherapistId, booking.AppointmentDate, booking.TimeSlot);
            await ExpirePendingBookingsAsync(booking.TherapistId, booking.AppointmentDate, booking.TimeSlot, booking.Id);

            booking.StripePaymentIntentId = stripePaymentIntentId;
            booking.PaymentStatus = "Paid";
            booking.PaidAt = DateTime.Now;

            if (booking.ExpiresAt.HasValue && booking.ExpiresAt.Value <= DateTime.Now)
            {
                booking.BookingStatus = "ManualReview";
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return;
            }

            var conflict = await _appointments.HasConflictAsync(booking.TherapistId, booking.AppointmentDate, booking.TimeSlot);
            if (conflict)
            {
                booking.BookingStatus = "ManualReview";
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return;
            }

            var appointment = new Appointment
            {
                PatientName = booking.ParticipantName,
                PatientPhone = booking.ParticipantPhone,
                PatientEmail = booking.ParticipantEmail,
                TherapistId = booking.TherapistId,
                ServiceId = booking.ServiceId,
                AppointmentDate = booking.AppointmentDate,
                TimeSlot = booking.TimeSlot,
                TherapyNotes = $"Center booking paid online. BookingId={booking.Id}. {booking.Notes}".Trim(),
                Status = AppointmentStatus.Confirmed,
                CreatedAt = DateTime.Now
            };

            _db.Appointments.Add(appointment);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                booking.BookingStatus = "ManualReview";
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return;
            }

            booking.AppointmentId = appointment.Id;
            booking.BookingStatus = "Confirmed";
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        public async Task MarkCheckoutExpiredAsync(string stripeSessionId)
        {
            var booking = await _db.CenterServiceBookings.FirstOrDefaultAsync(b => b.StripeSessionId == stripeSessionId);
            if (booking == null || booking.PaymentStatus == "Paid")
            {
                return;
            }

            booking.BookingStatus = "Expired";
            booking.PaymentStatus = "Expired";
            booking.ExpiresAt = DateTime.Now;
            await _db.SaveChangesAsync();
        }

        public async Task MarkCheckoutFailedAsync(int? bookingId, string stripePaymentIntentId, string failureMessage)
        {
            CenterServiceBooking? booking = null;

            if (bookingId.HasValue && bookingId.Value > 0)
            {
                booking = await _db.CenterServiceBookings.FirstOrDefaultAsync(b => b.Id == bookingId.Value);
            }

            booking ??= await _db.CenterServiceBookings.FirstOrDefaultAsync(b => b.StripePaymentIntentId == stripePaymentIntentId);

            if (booking == null || booking.PaymentStatus == "Paid")
            {
                return;
            }

            booking.StripePaymentIntentId = stripePaymentIntentId;
            booking.PaymentStatus = "Failed";
            booking.BookingStatus = "PaymentFailed";
            booking.ExpiresAt = DateTime.Now;

            if (!string.IsNullOrWhiteSpace(failureMessage))
            {
                booking.Notes = string.IsNullOrWhiteSpace(booking.Notes)
                    ? $"Payment failure: {failureMessage}"
                    : $"{booking.Notes}\nPayment failure: {failureMessage}";
            }

            await _db.SaveChangesAsync();
        }

        public async Task MarkCheckoutCanceledAsync(int bookingId)
        {
            var booking = await _db.CenterServiceBookings.FirstOrDefaultAsync(b => b.Id == bookingId);
            if (booking == null || booking.PaymentStatus == "Paid")
            {
                return;
            }

            booking.BookingStatus = "Canceled";
            booking.PaymentStatus = "Canceled";
            booking.ExpiresAt = DateTime.Now;
            await _db.SaveChangesAsync();
        }

        private async Task<(bool Available, string Message)> CheckAvailabilityCoreAsync(int therapistId, DateTime date, string timeSlot)
        {
            if (therapistId <= 0 || string.IsNullOrWhiteSpace(timeSlot))
            {
                return (false, "Vui long chon chuyen gia va khung gio.");
            }

            var hasConfirmedConflict = await _appointments.HasConflictAsync(therapistId, date.Date, timeSlot);
            if (hasConfirmedConflict)
            {
                return (false, "Khung gio nay da duoc dat. Vui long chon khung gio khac.");
            }

            var hasPendingBooking = await _db.CenterServiceBookings.AnyAsync(b =>
                b.TherapistId == therapistId &&
                b.AppointmentDate == date.Date &&
                b.TimeSlot == timeSlot &&
                b.BookingStatus == "PendingPayment" &&
                (!b.ExpiresAt.HasValue || b.ExpiresAt > DateTime.Now));

            if (hasPendingBooking)
            {
                return (false, "Khung gio nay dang duoc giu tam thoi. Vui long thu khung gio khac.");
            }

            return (true, "Khung gio kha dung.");
        }

        private async Task ExpirePendingBookingsAsync(int therapistId, DateTime appointmentDate, string timeSlot, int? excludeBookingId = null)
        {
            var staleBookings = await _db.CenterServiceBookings
                .Where(b =>
                    b.TherapistId == therapistId &&
                    b.AppointmentDate == appointmentDate &&
                    b.TimeSlot == timeSlot &&
                    b.BookingStatus == "PendingPayment" &&
                    b.ExpiresAt.HasValue &&
                    b.ExpiresAt.Value <= DateTime.Now)
                .ToListAsync();

            foreach (var staleBooking in staleBookings)
            {
                if (excludeBookingId.HasValue && staleBooking.Id == excludeBookingId.Value)
                {
                    continue;
                }

                staleBooking.BookingStatus = "Expired";
                staleBooking.PaymentStatus = "Expired";
            }

            if (staleBookings.Count > 0)
            {
                await _db.SaveChangesAsync();
            }
        }

        private async Task AcquireSlotLockAsync(int therapistId, DateTime appointmentDate, string timeSlot)
        {
            var resource = $"center-booking-slot:{therapistId}:{appointmentDate:yyyyMMdd}:{timeSlot}";
            await _db.Database.ExecuteSqlInterpolatedAsync($"EXEC sp_getapplock @Resource = {resource}, @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 10000;");
        }

        private async Task<string> GenerateTrackingCodeAsync()
        {
            for (var attempt = 0; attempt < 10; attempt++)
            {
                var code = PublicTrackingCodeGenerator.Generate("CBK");
                if (!await _db.CenterServiceBookings.AnyAsync(b => b.TrackingCode == code))
                {
                    return code;
                }
            }

            throw new InvalidOperationException("Could not generate a unique booking tracking code.");
        }
    }
}
