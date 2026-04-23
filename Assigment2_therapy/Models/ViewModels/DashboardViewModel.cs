using Montra.DAL.Entities;

namespace Assigment2_therapy.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalTherapists { get; set; }
        public int TotalAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<Appointment> RecentAppointments { get; set; } = new();
    }

    public class TherapistDashboardViewModel
    {
        public Therapist? Therapist { get; set; }
        public List<Appointment> TodayAppointments { get; set; } = new();
        public int TotalAssigned { get; set; }
        public int PendingCount { get; set; }
        public int CompletedCount { get; set; }
    }

    public class CenterBookingManagementViewModel
    {
        public string? BookingStatus { get; set; }
        public string? Search { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public int TotalBookings { get; set; }
        public int PendingPaymentCount { get; set; }
        public int ConfirmedCount { get; set; }
        public int ExpiredCount { get; set; }
        public int PaymentFailedCount { get; set; }
        public int ManualReviewCount { get; set; }
        public List<string> StatusOptions { get; set; } = new();
        public List<CenterServiceBooking> Bookings { get; set; } = new();
    }

    public class ReferralQueueManagementViewModel
    {
        public string? Status { get; set; }
        public string? Search { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int TotalReferrals { get; set; }
        public int NewCount { get; set; }
        public int UnderReviewCount { get; set; }
        public int ContactedCount { get; set; }
        public int ScheduledCount { get; set; }
        public int ClosedCount { get; set; }
        public List<string> StatusOptions { get; set; } = new();
        public List<ReferralRequest> Referrals { get; set; } = new();
    }
}
