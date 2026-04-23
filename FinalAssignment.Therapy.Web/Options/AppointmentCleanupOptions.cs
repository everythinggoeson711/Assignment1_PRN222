namespace FinalAssignment.Therapy.Web.Options;

public class AppointmentCleanupOptions
{
    public const string SectionName = "AppointmentCleanup";

    public int PendingPaymentLifetimeHours { get; set; } = 24;

    public int ScanIntervalSeconds { get; set; } = 60;
}