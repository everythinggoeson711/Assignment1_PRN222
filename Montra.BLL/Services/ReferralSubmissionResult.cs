namespace Montra.BLL.Services
{
    public class ReferralSubmissionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string TrackingCode { get; set; } = string.Empty;
    }
}