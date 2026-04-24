namespace FinalAssignment.Therapy.Application.Models;

public class BookingResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string QrCodeUrl { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string TrackingCode { get; set; } = string.Empty;
    public int AppointmentId { get; set; }
}

/// <summary>Alias used by views that reference BookingServiceResult.</summary>
public class BookingServiceResult : BookingResult { }

public class SePayWebhookData
{
    public long Id { get; set; }
    public string? Gateway { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? AccountNumber { get; set; }
    public string? Content { get; set; }
    public string? TransferType { get; set; }
    public long TransferAmount { get; set; }
    public string? ReferenceCode { get; set; }
}
