using Microsoft.EntityFrameworkCore;
using Montra.DAL.Context;

namespace Montra.BLL.Services
{
    public static class TrackingCodeMaintenanceService
    {
        public static async Task BackfillMissingCodesAsync(AppDbContext db)
        {
            var referrals = await db.ReferralRequests
                .Where(r => string.IsNullOrWhiteSpace(r.TrackingCode))
                .ToListAsync();

            foreach (var referral in referrals)
            {
                referral.TrackingCode = await GenerateUniqueReferralCodeAsync(db);
            }

            var bookings = await db.CenterServiceBookings
                .Where(b => string.IsNullOrWhiteSpace(b.TrackingCode))
                .ToListAsync();

            foreach (var booking in bookings)
            {
                booking.TrackingCode = await GenerateUniqueBookingCodeAsync(db);
            }

            if (referrals.Count > 0 || bookings.Count > 0)
            {
                await db.SaveChangesAsync();
            }
        }

        private static async Task<string> GenerateUniqueReferralCodeAsync(AppDbContext db)
        {
            for (var attempt = 0; attempt < 10; attempt++)
            {
                var code = PublicTrackingCodeGenerator.Generate("REF");
                if (!await db.ReferralRequests.AnyAsync(r => r.TrackingCode == code))
                {
                    return code;
                }
            }

            throw new InvalidOperationException("Could not backfill a unique referral tracking code.");
        }

        private static async Task<string> GenerateUniqueBookingCodeAsync(AppDbContext db)
        {
            for (var attempt = 0; attempt < 10; attempt++)
            {
                var code = PublicTrackingCodeGenerator.Generate("CBK");
                if (!await db.CenterServiceBookings.AnyAsync(b => b.TrackingCode == code))
                {
                    return code;
                }
            }

            throw new InvalidOperationException("Could not backfill a unique booking tracking code.");
        }
    }
}